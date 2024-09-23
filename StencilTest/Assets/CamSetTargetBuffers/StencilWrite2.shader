Shader "Custom/StencilWrite2"
{
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass
	{
		Stencil
	{
		Ref 1         // 写入模板值1
		Comp always   // 总是写入
		Pass replace  // 替换当前模板值
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
		return fixed4(0, 1, 0, 1); // 显示为绿色用于调试
	}
		ENDCG
	}
	}
}
