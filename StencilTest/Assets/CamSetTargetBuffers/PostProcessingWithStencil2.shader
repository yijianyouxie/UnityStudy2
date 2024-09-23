Shader "Custom/PostProcessingWithStencil2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_StencilTex("Stencil Texture", 2D) = "white" {} // Stencil Buffer
	}

		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass
	{
		CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

		sampler2D _MainTex;
	sampler2D _StencilTex;

	fixed4 frag(v2f_img i) : SV_Target
	{
		// 读取 stencil 信息
		float stencilValue = tex2D(_StencilTex, i.uv).r;
	//return tex2D(_StencilTex, i.uv);

	if (stencilValue < 1.0) // 非模板区域
	{
		return fixed4(1, 0, 0, 1); // 将非模板区域渲染为红色
	}
	else
	{
		return tex2D(_MainTex, i.uv); // 保持原始颜色
	}
	}
		ENDCG
	}
	}
}
