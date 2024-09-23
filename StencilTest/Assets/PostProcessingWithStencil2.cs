using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PostProcessingWithStencil2 : MonoBehaviour
{
    public Material postProcessingMaterial; // 后处理材质
    public Material stencilMaskMaterial;
    private RenderTexture targetRT;
    public RenderTexture stencilRT;
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        targetRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        stencilRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = targetRT;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 将渲染结果传给后处理材质，并输出到屏幕
        if (stencilMaskMaterial != null)
        {
            Graphics.Blit(source, destination, stencilMaskMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
    private void OnPreRender()
    {
        cam.targetTexture = targetRT;
    }
    private void OnPostRender()
    {
        //cam.targetTexture = null;
        if (postProcessingMaterial != null)
        {
            Graphics.Blit(targetRT, stencilRT, postProcessingMaterial);

            Shader.SetGlobalTexture("_StencilMask", stencilRT);
        }
        //else
        //{
        //    Graphics.Blit(targetRT, null);
        //}
    }
}
