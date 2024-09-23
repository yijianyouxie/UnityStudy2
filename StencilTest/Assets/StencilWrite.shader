Shader "Custom/StencilWrite"
{
	Properties
	{
		/*[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8*/
		_Stencil("Stencil ID", Float) = 0
		/*_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15*/
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass
	{
		// 写入Stencil值
		Stencil
	{
		Ref [_Stencil]         // 写入Stencil的值为1
		Comp always   // 总是写入
		Pass replace  // 替换现有Stencil值
	}

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// 不需要真正改变颜色
		return float4(0, 1, 0, 1);
	}
		ENDCG
	}
	}
}
