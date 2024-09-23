using UnityEngine;

public class StencilPostProcessingWithRT : MonoBehaviour
{
    public Material postProcessingMaterial; // 后处理材质
    public Material StencilProcessMat;

    public RenderTexture colorRT;
    public RenderTexture depthStencilRT; // 带有 Stencil 的 RT
    private Camera mainCamera;

    public RenderTexture postProcessRT;

    void Start()
    {
        mainCamera = Camera.main;

        // 创建颜色缓冲区的 RenderTexture
        colorRT = new RenderTexture(Screen.width, Screen.height, 0);

        // 创建带有深度和Stencil缓冲区的 RenderTexture
        depthStencilRT = new RenderTexture(Screen.width, Screen.height, 24); // 24位包含深度和Stencil

        // 用于后续后处理的RenderTexture
        postProcessRT = new RenderTexture(Screen.width, Screen.height, 0);

        // 设置摄像机渲染到这两个目标缓冲区
        mainCamera.SetTargetBuffers(colorRT.colorBuffer, depthStencilRT.depthBuffer);
    }
    private void OnPreRender()
    {
        // 将摄像机的渲染目标设为带有颜色和深度缓冲的 RenderTexture
        mainCamera.SetTargetBuffers(colorRT.colorBuffer, depthStencilRT.depthBuffer);
    }

    void OnPostRender()
    {
        // 设置渲染目标为 postProcessRT (无 stencil 信息的颜色缓冲区)
        Graphics.SetRenderTarget(postProcessRT);

        // 清除 postProcessRT 颜色缓冲区
        GL.Clear(true, true, new Color(0, 0, 0, 0));

        // 使用 stencil buffer 处理材质，从 depthStencilRT 提取 stencil 信息
        Graphics.SetRenderTarget(postProcessRT.colorBuffer, depthStencilRT.depthBuffer);

        // 将 stencil buffer 的内容通过处理材质转换为颜色信息
        Graphics.Blit(colorRT, /*postProcessRT,*/ StencilProcessMat);

        postProcessingMaterial.SetTexture("_StencilTex", postProcessRT);
        //Graphics.Blit(colorRT, null as RenderTexture, postProcessingMaterial);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //Graphics.SetRenderTarget(null);
        // 使用 postProcessRT 中的 stencil 信息做进一步后处理
        //postProcessingMaterial.SetTexture("_StencilTex", postProcessRT);

        // 将最终效果渲染到屏幕上
        Graphics.Blit(colorRT, dest, postProcessingMaterial);
    }

    private void OnDisable()
    {
        mainCamera.targetTexture = null;
    }
}