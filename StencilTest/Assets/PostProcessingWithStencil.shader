Shader "Custom/PostProcessingWithStencil" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+1" }
		Pass
	{
		// 只处理Stencil值为0的区域，即没有被标记的区域
		Stencil
	{
		Ref 1         // 参考值1
		//Comp notequal // 当Stencil不等于1时，处理区域
		Comp equal // 当Stencil不等于1时，处理区域
	}

		CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

		sampler2D _MainTex;

	fixed4 frag(v2f_img i) : SV_Target
	{
		// 将没有标记的区域渲染为红色
		/*float stencilValue = UnityGetFragmentStencil();
		return fixed4(stencilValue, stencilValue, stencilValue, 1.0);*/
		// 读取当前屏幕纹理
		fixed4 col = tex2D(_MainTex, i.uv);
		// 调试: 通过返回颜色来查看Stencil值
		// 如果Stencil值不为1，则返回红色，Stencil值为1则返回原始颜色
		//return col * step(0.5, col.a); // 透明度检查
		return fixed4(1.0, 1.0, 0.0, 1.0);
	}
		ENDCG
	}
	}
}
