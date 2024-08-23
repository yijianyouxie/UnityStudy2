using UnityEngine;
using System.Collections;


[ExecuteAlways]
public class MirrorReflection : MonoBehaviour
{
	public bool _DisablePixelLights = true;
	public int _TextureSize = 1024;
    // public int __TextureDepthSize = 256;
    public float __ClipPlaneOffset = 0.07f;
    //	public bool Depth;
    public LayerMask __ReflectLayers = -1;
    public LayerMask __ReflectDiamondLayers = -1;
    //    public LayerMask __DepthLayers = -1;
    public Shader DiamondShader;

    public CameraClearFlags ClearFlags = CameraClearFlags.Skybox;

    private Hashtable __ReflectionCameras = new Hashtable();

    public RenderTexture __ReflectionTexture = null;
    public RenderTexture __ReflectionDiamondTexture = null;
    // private RenderTexture __DepthTexture = null;
    private int __OldReflectionTextureSize = 0;
    Camera reflectionCamera;
    private static bool s_InsideRendering = false;




    public void OnWillRenderObject()
	{
		var rend = GetComponent<Renderer>();
		if (!enabled || !rend || !rend.sharedMaterial || !rend.enabled)
			return;
		
		Camera cam = Camera.current;
		if( !cam )
			return;
		       
		if( s_InsideRendering )
			return;
		s_InsideRendering = true;
		
		
		CreateMirror( cam, out reflectionCamera );

        ////// reflectionCamera.SetReplacementShader(DiamondShader, "RenderType");
        // reflectionCamera.SetReplacementShader(DiamondShader2, "RenderType");

        Vector3 pos = transform.position;
		Vector3 normal = transform.up;
		

		int oldPixelLightCount = QualitySettings.pixelLightCount;
		if( _DisablePixelLights )
			QualitySettings.pixelLightCount = 0;
		
		UpdateCameraModes( cam, reflectionCamera );

		float d = -Vector3.Dot (normal, pos) - __ClipPlaneOffset;
		Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);
		
		Matrix4x4 reflection = Matrix4x4.zero;
		CalculateMatrix (ref reflection, reflectionPlane);
		Vector3 oldpos = cam.transform.position;
		Vector3 newpos = reflection.MultiplyPoint( oldpos );
		reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
		

		Vector4 clipPlane = CameraSpacePlane( reflectionCamera, pos, normal, 1.0f );
		Matrix4x4 projection = cam.CalculateObliqueMatrix(clipPlane);
		reflectionCamera.projectionMatrix = projection;

       

        reflectionCamera.cullingMask = ~(1<<4) & __ReflectLayers.value; 
		reflectionCamera.targetTexture = __ReflectionTexture;
		GL.SetRevertBackfacing (true);


     
        reflectionCamera.transform.position = newpos;
		Vector3 euler = cam.transform.eulerAngles;
		reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
        //  reflectionCamera.SetReplacementShader(DiamondShader, "RenderType");


        reflectionCamera.clearFlags = ClearFlags;

        if(ClearFlags == CameraClearFlags.Depth)
        {
            reflectionCamera.clearFlags = CameraClearFlags.Color;
        }

        if (ClearFlags == CameraClearFlags.Nothing)
        {
            reflectionCamera.clearFlags = CameraClearFlags.Color;
        }

        if (ClearFlags == CameraClearFlags.SolidColor)
        {
            reflectionCamera.clearFlags = CameraClearFlags.Color;
        }

        reflectionCamera.ResetReplacementShader();

        reflectionCamera.backgroundColor = new Color(0, 0, 0, 0);

        reflectionCamera.Render();


        reflectionCamera.transform.position = oldpos;

       
		Material[] materials = rend.sharedMaterials;





        foreach ( Material mat in materials ) {
			if( mat.HasProperty("_ReflectionTex") )
				mat.SetTexture( "_ReflectionTex", __ReflectionTexture );
		}
        reflectionCamera.SetReplacementShader(DiamondShader, "RenderType");

        reflectionCamera.clearFlags = CameraClearFlags.Color;
        reflectionCamera.backgroundColor = new Color(0, 0, 0, 0);
        reflectionCamera.cullingMask = ~(1 << 4) & __ReflectDiamondLayers.value;
            reflectionCamera.targetTexture = __ReflectionDiamondTexture;

           reflectionCamera.Render();


        
        foreach (Material mat in materials)
        {
            if (mat.HasProperty("_ReflectionDiamondTex"))
                mat.SetTexture("_ReflectionDiamondTex", __ReflectionDiamondTexture);
        }


        /*  if (Depth) { 
        reflectionCamera.cullingMask = ~(1 << 4) & __DepthLayers.value;
        reflectionCamera.targetTexture = __DepthTexture;
        reflectionCamera.Render();
        foreach (Material mat in materials)
        {
            if (mat.HasProperty("_DepthTex"))
                mat.SetTexture("_DepthTex", __DepthTexture);
        }

        }*/

        GL.SetRevertBackfacing(false);
        if ( _DisablePixelLights )
			QualitySettings.pixelLightCount = oldPixelLightCount;
		
		s_InsideRendering = false;
	}
	
	void OnDisable()
	{
		if( __ReflectionTexture ) {
			DestroyImmediate( __ReflectionTexture );
			__ReflectionTexture = null;
		}
		foreach( DictionaryEntry kvp in __ReflectionCameras )
			DestroyImmediate( ((Camera)kvp.Value).gameObject );
		__ReflectionCameras.Clear();
	}
	
	
	private void UpdateCameraModes( Camera src, Camera dest )
	{
		if( dest == null )
			return;
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;        
		if( src.clearFlags == CameraClearFlags.Skybox )
		{
			Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
			if( !sky || !sky.material )
			{
				mysky.enabled = false;
			}
			else
			{
				mysky.enabled = true;
				mysky.material = sky.material;
			}
		}

		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}
	

	private void CreateMirror( Camera currentCamera, out Camera reflectionCamera )
	{
		reflectionCamera = null;

        
        if ( !__ReflectionTexture || __OldReflectionTextureSize != _TextureSize )
		{
			if( __ReflectionTexture )
				DestroyImmediate( __ReflectionTexture );

			__ReflectionTexture = new RenderTexture( _TextureSize, _TextureSize, 16 );
            __ReflectionDiamondTexture = new RenderTexture(_TextureSize, _TextureSize, 16);
         //   __DepthTexture = new RenderTexture(__TextureDepthSize, __TextureDepthSize, 16);




            __ReflectionTexture.name = "__MirrorReflection" + GetInstanceID();
			__ReflectionTexture.isPowerOfTwo = true;
			__ReflectionTexture.hideFlags = HideFlags.DontSave;
            __ReflectionTexture.useMipMap = false;


            __ReflectionDiamondTexture.name = "__MirrorDiamondReflection" + GetInstanceID();
            __ReflectionDiamondTexture.isPowerOfTwo = true;
            __ReflectionDiamondTexture.hideFlags = HideFlags.DontSave;
            __ReflectionDiamondTexture.useMipMap = false;

            //  __DepthTexture.name = "__MirrorDepth" + GetInstanceID();
            //   __DepthTexture.isPowerOfTwo = true;
            //   __DepthTexture.hideFlags = HideFlags.DontSave;
            __OldReflectionTextureSize = _TextureSize;
		}
       
        reflectionCamera = __ReflectionCameras[currentCamera] as Camera;
        
        if ( !reflectionCamera ) 
		{
			GameObject go = new GameObject( "Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox) );
			reflectionCamera = go.GetComponent<Camera>();
			reflectionCamera.enabled = false;
			reflectionCamera.transform.position = transform.position;
			reflectionCamera.transform.rotation = transform.rotation;
			reflectionCamera.gameObject.AddComponent<FlareLayer>();
            
            go.hideFlags = HideFlags.HideAndDontSave;
			__ReflectionCameras[currentCamera] = reflectionCamera;
          //  reflectionCamera.SetReplacementShader(DiamondShader, "Jewel");
        //  reflectionCamera.SetReplacementShader(DiamondShader2, "Opaque");
        }
       
    }

	private static float sgn(float a)
	{
		if (a > 0.0f) return 1.0f;
		if (a < 0.0f) return -1.0f;
		return 0.0f;
	}
	

	private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * __ClipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint( offsetPos );
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
	}
	

	private static void CalculateMatrix (ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
		reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
		reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
		reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);
		
		reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
		reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
		reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
		reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);
		
		reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
		reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
		reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
		reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);
		
		reflectionMat.m30 = 0F;
		reflectionMat.m31 = 0F;
		reflectionMat.m32 = 0F;
		reflectionMat.m33 = 1F;
	}
}