using UnityEngine;

public class RenderWithTargetBuffers : MonoBehaviour
{
    public Camera mainCamera;            // 主摄像机
    public Material postProcessingMaterial; // 后处理材质

    private RenderTexture colorRT;       // 用于颜色输出的RenderTexture
    private RenderTexture depthStencilRT; // 用于深度和Stencil的RenderTexture

    void Start()
    {
        //// 创建带有深度和模板缓冲区的 RenderTexture
        //colorRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        //depthStencilRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);

        //// 确保启用 Stencil Buffer
        ////depthStencilRT.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt;

        //// 设置目标缓冲区
        //mainCamera.SetTargetBuffers(colorRT.colorBuffer, depthStencilRT.depthBuffer);
    }
    private int _currentScreenWidth = 0, _currentScreenHeight = 0;
    private void OnPreRender()
    {
        if (mainCamera == null)
            return;

        int screenWidth = (int)(Screen.width);
        int screenHeight = (int)(Screen.height);

        var needSetRenderTarget = true;

        if (screenWidth != _currentScreenWidth || screenHeight != _currentScreenHeight)
        {

            _currentScreenWidth = screenWidth;
            _currentScreenHeight = screenHeight;

            // 确保释放资源
            if (colorRT != null)
            {
                colorRT.Release();
            }

            if (depthStencilRT != null)
            {
                depthStencilRT.Release();
            }
            colorRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            depthStencilRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);

            needSetRenderTarget = false;
        }

        // 在rt创建后，下一帧才真正设置进去
        if (needSetRenderTarget)
        {
            mainCamera.SetTargetBuffers(colorRT.colorBuffer, depthStencilRT.depthBuffer);
        }

        //if (_editorCamera && editorColorRT)
        //{
        //    _editorCamera.SetTargetBuffers(editorColorRT.colorBuffer, editorDepthRT.depthBuffer);

        //    Graphics.Blit(editorDepthRT, editorDepthTex);
        //    Shader.SetGlobalTexture("_SceneDepthTex", editorDepthTex);

        //    Graphics.Blit(editorColorRT, editorSceneColorTex);
        //    Shader.SetGlobalTexture("_SceneColorTex", editorSceneColorTex);

        //    _editorCamera.fieldOfView = _mainCamera.fieldOfView;
        //}
    }

    private void OnPostRender()
    {
        mainCamera.targetTexture = null;
        if (postProcessingMaterial != null)
        {
            Graphics.Blit(colorRT, postProcessingMaterial);
        }
        //else
        //{
        //    Graphics.Blit(colorRT);
        //}
    }
    private void Update()
    {
        mainCamera.SetTargetBuffers(colorRT.colorBuffer, depthStencilRT.depthBuffer);
    }

    //void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    // 将自定义渲染的结果通过后处理材质进行处理
    //    if (postProcessingMaterial != null)
    //    {
    //        Graphics.Blit(colorRT, destination, postProcessingMaterial);
    //    }
    //    else
    //    {
    //        Graphics.Blit(colorRT, destination);
    //    }
    //}

    void OnDisable()
    {
        // 确保释放资源
        if (colorRT != null)
        {
            colorRT.Release();
        }

        if (depthStencilRT != null)
        {
            depthStencilRT.Release();
        }
    }
}
