

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace DiamondRender
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    //[ExecuteAlways]
    public class DiamondRenderer : MonoBehaviour
    {
        //        public JewelModel model;        
        public Color color = new Color(1,1,1,1);
        
        [Range(0, 3)]
        public float ColorIntensity = 1.7f;

        [Range(0, 1.5f)]
        public float LightTransmission = 0.5f;

        [Range(0, 1)]
        public float ColorByDepth = 0.1f;

        [Range(0,10)]
        public int MaxReflection = 4;

        [Range(1, 5)]
        public float RefractiveIndex = 1.6f;
        //      public int maxReflectionCount;
         Cubemap environment;
        //        public float refractiveIndex;
        //       public float baseReflection;
        //    public bool prism;

        // public bool mergeSimilarPlanes = true;

        //    public bool changeShaders;

        public bool autoCaptureEnvironment = false;
            public bool captureEnvironmentOnSetup = false;
            public int captureEnvironmentSize = 512;


            //    public ReflectionProbe _reflectionProbe;

            public MaterialPropertyBlock block;


        /*    public Shader ShaderCubeMap;
            public Shader ShaderReflectionProbe; */

        // calculated
        [SerializeField]
      //  [HideInInspector]
        float scale;
        [SerializeField]
  //      [HideInInspector]
        Texture2D shapeTexture;
        [SerializeField]
        [HideInInspector]
        int planeCount;

        Cubemap capturedEnvironment = null;
        Material mat;

         float Time_;

        //   public Transform CentrePivot;
        //    public Transform CentrePivotDiamond2;
        //    public Transform CentrePivotDiamond3;

        //  public float dots = 0.5f;

        //  public float distRPI;
        Vector3 MinPos;

        Vector3 MaxPos;

        [HideInInspector]
        public Vector4 CentreModel;

        //   public float RPI = 0;
        // public Camera cam;

        MeshRenderer MR;

        [HideInInspector]
        public Matrix4x4 m;




        private void Start()
        {
          
            MR = GetComponent<MeshRenderer>();

            if (block == null)
            {
                block = new MaterialPropertyBlock();
                MR.GetPropertyBlock(block);
                
            }
        }

        private void Enable()
        {
           
            MR = GetComponent<MeshRenderer>();
            mat = MR.sharedMaterial;

            if (block == null)
            {
                block = new MaterialPropertyBlock();
                  MR.GetPropertyBlock(block);
               
            }
        }

        private void Update()
        {

            if(block == null)
            {
                   block = new MaterialPropertyBlock();

                     MR.GetPropertyBlock(block);
                
            }




            // if(_reflectionProbe != null)
            //       mat.SetFloat("lightEstimation2",Mathf.Clamp( _reflectionProbe.intensity,0,0.5f));


            //    if (_reflectionProbe != null)
            //    {


            //    RPI = _reflectionProbe.intensity;


            //       distRPI =  0.65f - (RPI - 0.65f);

            //      if (_reflectionProbe.intensity > 0.65f)
            //       {
            //        RPI = RPI * (1 - _reflectionProbe.intensity);
            //      }

            //  RPI = Mathf.Clamp(RPI, 0, 0.5f);

            //  mat.SetFloat("lightEstimation2", RPI + (distRPI * -1f));
            //  block.SetFloat("lightEstimation2", RPI);

            //   }


            if (MR == null)
            {
                MR = GetComponent<MeshRenderer>();
            }

            if (mat == null)
                mat = MR.sharedMaterial;

            //    point =  cam.ScreenToWorldPoint(CentrePivotDiamond3.position);




            //    mat.SetVector("poinT", point);





            //  Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);


            m = MR.worldToLocalMatrix;

            //这三个分量代表平移，所以将所有点都平移，减去中心点
            m.m03 -= CentreModel.x;
            m.m13 -= CentreModel.y;
            m.m23 -= CentreModel.z;


            //     Matrix4x4 m2 = Matrix4x4.TRS(CentrePivotDiamond2.position, CentrePivotDiamond2.rotation, CentrePivotDiamond2.localScale);

            /// Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            /// 
            

            block.SetVector("CentreModel", CentreModel);
            block.SetFloat("ColorByDepth", ColorByDepth);
            block.SetColor("_Color", color); 
            block.SetFloat("ColorIntensity", ColorIntensity);
            block.SetFloat("lighttransmission", LightTransmission);
            block.SetFloat("_RefractiveIndex", RefractiveIndex);
            block.SetFloat("_MaxReflection", MaxReflection);
            block.SetMatrix("MatrixWorldToObject", m);

            if (shapeTexture != null) { 
            block.SetTexture("_ShapeTex", shapeTexture);
            block.SetFloat("_SizeX", shapeTexture.width);
            block.SetFloat("_SizeY", shapeTexture.height);
            block.SetFloat("_Scale", scale);
            block.SetFloat("_PlaneCount", planeCount);
            }

            MR.SetPropertyBlock(block);

            //   mat.SetMatrix("MatrixWorldToObject2", m2);

            if (mat == null)
             mat = MR.sharedMaterial;

            //   mat.SetVector("CentrePivotDiamond", CentrePivot.position);



//   mat.SetVector("CentrePivotDiamond2", CentrePivotDiamond2.position);
/*
            if (changeShaders) { 

            if (Time_ < 2)
            {
                Time_ += Time.deltaTime / 2;
                    
                    //    ShaderCubeMap = Shader.Find( "Custom/DiamondShader");

                    //    if (ShaderCubeMap != null)
                    //     {
                    //         mat.shader = ShaderCubeMap;
                    //    }

                    //     }
                    //          else
                    //          {
                    //         ShaderReflectionProbe = Shader.Find("Custom/DiamondShaderReflectionProbe");

                    if (ShaderReflectionProbe != null)
                {
                    mat.shader = ShaderReflectionProbe;
                }
                }
            }
            */
            








            if (autoCaptureEnvironment)
            {
                CaptureEnvironment();
            }
        }


        


        [ContextMenu("Setup")]
        public void Setup()
        {
            mat = MR.sharedMaterial;


            AnalyzeMesh();

            MeshRenderer mr = GetComponent<MeshRenderer>();

            if (mat == null)
                mat = mr.sharedMaterial;

            //      Material m = new Material(Shader.Find( "Custom/DiamondShader"));

            block.SetTexture("_ShapeTex", shapeTexture);
            block.SetFloat("_SizeX", shapeTexture.width);
            block.SetFloat("_SizeY", shapeTexture.height);
            block.SetFloat("_Scale", scale);
            block.SetFloat("_PlaneCount", planeCount);

            mr.SetPropertyBlock(block);

            //      m.SetColor("_Color", color);
            //    m.SetInt("_MaxReflection", maxReflectionCount);
            //     m.SetTexture("_Environment", environment);
            //     m.SetFloat("_RefractiveIndex", refractiveIndex);
            //     m.SetFloat("_BaseReflection", baseReflection);

            mr.material = mat;

            if (captureEnvironmentOnSetup)
            {
                CaptureEnvironment();
            }
        }

        [ContextMenu("CaptureEnvironment")]
        public void CaptureEnvironment()
        {
            Material m = GetComponent<MeshRenderer>().sharedMaterial;

            if( m == null )
            {
                Debug.LogWarning("Material is not setup yet. please do Setup first.");
                return;
            }

            if (capturedEnvironment == null)
            {
                capturedEnvironment = new Cubemap(captureEnvironmentSize, TextureFormat.ARGB32, false);
            }

            Camera cameraComponent = GetComponent<Camera>();

            bool temporaryCameraComponent = false;
            if (cameraComponent == null)
            {
                cameraComponent = gameObject.AddComponent<Camera>();
                temporaryCameraComponent = true;
            }

            cameraComponent.RenderToCubemap(capturedEnvironment);

            environment = capturedEnvironment;

            m.SetTexture("_Environment", capturedEnvironment);

#if UNITY_EDITOR
            if ( temporaryCameraComponent)
            {
                DestroyImmediate(cameraComponent);
            }
#endif
        }

        [ContextMenu("ApplyNumericParameters")]
        public void ApplyNumericParameters()
        {
            Material m = GetComponent<MeshRenderer>().sharedMaterial;




            m.SetInt("_SizeX", shapeTexture.width);
            m.SetInt("_SizeY", shapeTexture.height);
            m.SetFloat("_Scale", scale);
            m.SetInt("_PlaneCount", planeCount);
            m.SetColor("_Color", color);
            //     m.SetInt("_MaxReflection", maxReflectionCount);
            //     m.SetTexture("_Environment", environment);
            //       m.SetFloat("_RefractiveIndex", refractiveIndex);
            //       m.SetFloat("_BaseReflection", baseReflection);
        }

        bool AnalyzeMesh()
        {
            Mesh sourceMesh = GetComponent<MeshFilter>().sharedMesh;

            if (sourceMesh == null)
            {
                return false;
            }

            Vector3[] vertices = sourceMesh.vertices;//顶点坐标数组
            Vector3[] normals = sourceMesh.normals;//顶点法线数组
            int[] indices = sourceMesh.GetIndices(0);//组成mesh的顶点的索引数组




            //获取拓扑类型
            MeshTopology topology = sourceMesh.GetTopology(0);

            //模型的几何中心点
            CentreModel = new Vector4(1, 1, 1, 1);

            MaxPos = new Vector4(-9999999, -9999999, -9999999, 1);
            MinPos = new Vector4(9999999, 9999999, 9999999, 1);

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].x < MinPos.x)
                {
                    MinPos.x = vertices[i].x;
                }

                if (vertices[i].y < MinPos.y)
                {
                    MinPos.y = vertices[i].y;
                }

                if (vertices[i].z < MinPos.z)
                {
                    MinPos.z = vertices[i].z;
                }



                if (vertices[i].x > MaxPos.x)
                {
                    MaxPos.x = vertices[i].x;
                }

                if (vertices[i].y > MaxPos.y)
                {
                    MaxPos.y = vertices[i].y;
                }

                if (vertices[i].z > MaxPos.z)
                {
                    MaxPos.z = vertices[i].z;
                }

            }

            //上边循环找出模型的最大最小坐标后，计算几何中心点
            CentreModel.x = (MaxPos.x + MinPos.x) / 2;
            CentreModel.y = (MaxPos.y + MinPos.y) / 2;
            CentreModel.z = (MaxPos.z + MinPos.z) / 2;
            //平移中心点到上面计算出的新中心点
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].x = vertices[i].x - CentreModel.x;
                vertices[i].y = vertices[i].y - CentreModel.y;
                vertices[i].z = vertices[i].z - CentreModel.z;
            }

            //计算所有顶点到几何中心点的长度，并取最大的那个长度
            scale = 0.0f;
            // calc scale
            for (int i = 0; i < vertices.Length; ++i)
            {
                // 5% margin
                scale = Mathf.Max(vertices[i].magnitude * 1.05f, scale);
                //Debug.LogError("========scale:" + scale + " magnitude:" + vertices[i].magnitude);
            }

            int texSize = 4;
            Color[] planes = null;

            int stride = 3;
            if (topology == MeshTopology.Triangles)
            {
                stride = 3;
            }
            else if (topology == MeshTopology.Quads)
            {
                stride = 4;
            }
            else
            {
                // no support
                Debug.LogError("unsupported mesh topology detected : " + topology.ToString());
            }
            /*
            if (!mergeSimilarPlanes)
            {
                // old version (maybe better for some case?)
                planeCount = indices.Length / stride;

                while (texSize * texSize < planeCount)
                {
                    texSize *= 2;
                }

                planes = new Color[texSize * texSize];
                for (int i = 0; i < planeCount; i++)
                {
                    int index = i * stride;

                    int vertIndex = indices[index];
                    Vector3 primaryPosition = vertices[vertIndex];
                    Vector3 primaryNormal = normals[vertIndex];

                    Color packedPlane = PackPlaneIntoColor(primaryPosition, primaryNormal, scale);
                    planes[i] = packedPlane;
                    
                }
             //   print(1);
            }
            else
            {

                */



                // new version (optimized)
                List<Color> tmpPlanes = new List<Color>();
                //计算面数。顶点组织数据除以拓扑结构的几个顶点构成一个面
                int faceCount = indices.Length / stride;

                for (int i = 0; i < faceCount; i++)
                {
                    int index = i * stride;
                    //以三个顶点组成一个面为例，获取小组中的第一个作为主要点，通过此点获取位置和法线
                    int vertIndex = indices[index];
                    Vector3 primaryPosition = vertices[vertIndex];
                    Vector3 primaryNormal = normals[vertIndex];
                    //根据位置，发现和缩放，构成一个color
                    Color packedPlane = PackPlaneIntoColor(primaryPosition, primaryNormal, scale);

                    bool duplicated = false;
                    foreach(Color c in tmpPlanes)
                    {
                        if( c == packedPlane )
                        {
                            Debug.LogError("========duplicated，i:" + i);
                            duplicated = true;
                            break;
                        }
                    }
                    //去除掉重复的颜色值
                    if( !duplicated )
                    {
                        tmpPlanes.Add(packedPlane);
                    }
                }
                
                planeCount = tmpPlanes.Count;
                while (texSize * texSize < planeCount)
                {
                    texSize *= 2;
                }
                planes = new Color[texSize * texSize];
                for( int i=0; i<tmpPlanes.Count; ++i )
                {
                    planes[i] = tmpPlanes[i];
                }
            

            //            if( shapeTexture == null)
            // must new everytime to prevent being shared...
            {
                shapeTexture = new Texture2D(texSize, texSize);
                shapeTexture.filterMode = FilterMode.Point;
            }
            shapeTexture.Resize(texSize, texSize);
            shapeTexture.SetPixels(planes);
            shapeTexture.Apply();
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(shapeTexture, "Assets/SuperRealisticDiamondShaders/ShapeTextures/" + "_" + System.DateTime.Now.ToString("yyyy-MM-dd") + "_ " + System.DateTime.Now.Hour + "_ " + System.DateTime.Now.Minute + Random.Range(-99999,99999) + " shapeTexture_.asset"); // save the modified model
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

#endif
            return true;
        }
        /// <summary>
        /// 根据顶点位置和法线，打包成一个颜色值
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="in_scale"></param>
        /// <returns></returns>
        static Color PackPlaneIntoColor(Vector3 position, Vector3 normal, float in_scale)
        {
            Color retval;
            //法线肯定是归一化的在[-1,1],转化到[0,1]
            retval.r = (normal.x + 1.0f) * 0.5f;
            retval.g = (normal.y + 1.0f) * 0.5f;
            retval.b = (normal.z + 1.0f) * 0.5f;

            //    retval.a = dots;
            //这里获得的是顶点方向在法线方向上的投影长度
            //然后这个长度除以所有顶点到几何中心点的长度中的最大长度
            //Vector3.Dot(position, normal)这一句的几何意义是两个向量的长度乘以两个向量的cos值，因为position的最大值是in_scale
            //cos值的最大值也是1，所以这个a值的最大值是1
            //Vector3.Dot(position, normal)表示的是顶点的本地坐标在法线方向上的投影长度
            //这个.a表示的是沿着法线方向，此平面距离原点的距离
            retval.a = Vector3.Dot(position, normal) / in_scale;//顶点本地坐标和法线的点积除以缩放值

            if (retval.a < 0 || retval.a > 1.0f)
            {
                // error
                //    Debug.LogError("invalid model scale or vertex position detected...");
            }

            return retval;
        }
    }
}
 