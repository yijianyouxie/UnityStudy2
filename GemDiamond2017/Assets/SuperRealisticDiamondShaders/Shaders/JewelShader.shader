﻿
Shader "Diamonds/DiamondShader" {
	Properties {

		//_Color ("Color", Color) = (1,1,1,0.1)
		_ShapeTex("ShapeTex", 2D) = "white" {}

		//_Scale("Scale", Float) = 1
		_SizeX("SizeX", Int) = 16
		_SizeY("SizeY", Int) = 16
		_PlaneCount("PlaneCount", Int) = 6
		//_MaxReflection("MaxReflection", Int) = 3
		_Environment ("Environment", CUBE) = "white" {}
		MipLevel("MipLevel", Range(0.0, 10.0)) = 0.0

		_RefractiveIndex("RefractiveIndex", Range(1,5)) = 1.5

		[KeywordEnum( CubeMap, ReflectionProbe)] _CubeMapMode("CubeMapMode", Float) = 0
		_BaseReflection("BaseReflection", Range(0,1)) = 0.5
		Dispersion("Dispersion", Range(0,2)) = 1.2
		[Space(10)]
		DispersionR("DispersionR", Range(-0.5,1)) = 0.68
		DispersionG("DispersionG", Range(-0.5,1)) = 0.4
		DispersionB("DispersionB", Range(-0.5,1)) = 0.146

		[Space(10)]
		DispersionIntensity("DispersionIntensity", Range(0,10)) = 1
				 
		TotalInternalReflection("TotalInternalReflection", Range(0,2)) = 1
		Spec("specular",Range(0,10)) = 1

		[Space(20)]
		//_Scale("Scale", Float) = 1
				
		Brightness("Brightness",Range(0,2)) = 1
		Power("Power",Range(0,2)) = 1
		Contrast("Contrast",Range(0,2)) = 1
		_Disaturate("_Disaturate",Range(0,2)) = 1
		_Min("Min",Range(-1,1)) = 0
		_Max("Max",Range(0,2)) = 1
		PostExposure("PostExposure",Float) = 1

		ReflectionCube("ReflectionCube", CUBE) = "black" {}
		NormalMap("NormalMap", 2D) = "bump" {}
		_Specular("SpecularMap", 2D) = "white" {}
		DifuseMask("ReflectionMask", 2D) = "black" {}
	}
	SubShader{

		Tags { "RenderType" = "Jewel" }
		LOD 200

		Pass
		{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
			#pragma exclude_renderers gles
			#pragma multi_compile _CUBEMAPMODE_CUBEMAP  _CUBEMAPMODE_REFLECTIONPROBE

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "JewelShaderInc.cginc"
			#include "Lighting.cginc"

			sampler2D DifuseMask;
			uniform float4 DifuseMask_ST;
			sampler2D _Specular;
			float4 _Specular_ST;
			uniform float4 NormalMap_ST;
			uniform sampler2D NormalMap;
			float  Spec;
			sampler2D _Pos;
			float Reflection;
			float Contrast;
			float PostExposure;
			float _Min;
			float _Max;
			float _Disaturate;
			float DispersionHueHandler;
			//	float lightEstimation;
			float ID_;
			float FresnelPower;
			float FresnelScale;
			float ior_r = 2.408;
			float ior_g = 2.424;
			float ior_b = 2.432;
			float4 _Color;
			float lighttransmission;

			fixed4 frag(v2f i) : SV_Target
			{
				float3 cameraLocalPos;
				
				cameraLocalPos = i.Pos2;

				float3 pos = i.Pos;
				//获得从摄像机指向顶点的向量
				float3 localRay = normalize(pos - cameraLocalPos) ;

				float3 normal = i.Normal;
				float4 plane = float4(normal, dot(pos, normal));

				float reflectionRate = 0;
				//float reflectionRate2 = 0;
				float3 reflectionRay;
				float3 refractionRay;
								
				float tmpR = _RefractiveIndex;

				//tmpR = tmpR * 2;
				float  PlaneNull;

				/*
				--Pos 相对于几何中心点的本地坐标
				--PassCount
				--rayNormalized 摄像机指向顶点的向量
				--TriangleNormal 三角面的发现
				--startSideRelativeRefraction 起始侧的相对折射
				--reflectionRate 反射率
				--reflectionRate2 反射率2 【没有用到】
				--reflectionRay 反射方向 【没有用到】
				--refractionRay 折射方向
				--HorizontalElementSquared
				*/
				CollideRayWithPlane(0,localRay, plane, 1.0/tmpR, reflectionRate, /*reflectionRate2,*/ reflectionRay, refractionRay, PlaneNull);

				/*
				--pos 相对于几何中心点的本地坐标
				--refractionRay 折射方向
				--tmpR 折射率
				--_Color 外部传入的颜色
				--lighttransmission 光透射
				*/
				//获得折射颜色
				float4 refractionColor = GetColorByRay(pos, refractionRay, tmpR, 0, _Color, lighttransmission);
				refractionColor.w = 1.0;
				//世界空间的从点指向摄像机方向
				float3 _worldViewDir = UnityWorldSpaceViewDir(i.worldPos);
				_worldViewDir = normalize(_worldViewDir);
				//计算菲涅尔
				float fresnelNdotV5 = dot(normal, _worldViewDir);
				float fresnelNode5 = (1 * pow(1.0 - fresnelNdotV5,1));
				//世界空间的反射
				//实际没有用到，实际用的是通过法线贴图算出来的
				//float3 _worldReflection = reflect(-_worldViewDir, normal);
				//这里用到切线空间到世界空间是为了解析法线贴图
				float3 _worldTangent = i.tangent.xyz;
				float3 _worldNormal = i.WorldNormal.xyz;
				float3 _worldBitangent = i.WorldBitangent.xyz;
				float3 tanToWorld0 = float3(_worldTangent.x, _worldBitangent.x, _worldNormal.x);
				float3 tanToWorld1 = float3(_worldTangent.y, _worldBitangent.y, _worldNormal.y);
				float3 tanToWorld2 = float3(_worldTangent.z, _worldBitangent.z, _worldNormal.z);

				float2 uvNormal = i.uv * NormalMap_ST.xy + NormalMap_ST.zw;
				//世界空间的反射，这里采样的代码有三个相同的，可以先一次采样出来，然后在后边使用
				float3 worldRefl3 = reflect(-_worldViewDir, float3(dot(tanToWorld0, UnpackNormal(tex2D(NormalMap, uvNormal))), dot(tanToWorld1, UnpackNormal(tex2D(NormalMap, uvNormal))), dot(tanToWorld2, UnpackNormal(tex2D(NormalMap, uvNormal)))));

				//高光贴图算高光
				float spec_ = tex2D(_Specular, i.uv * _Specular_ST.xy + _Specular_ST.zw) * Spec;

				//反射颜色。根据反射立方体计算得出
				float4 reflectionColor = texCUBE(ReflectionCube, worldRefl3) * spec_ * fresnelNode5;
				 
				//获取色调，色相，但是这个后面没有用到
				float Hue = rgb2hsv(refractionColor.rgb).r;
				//计算灰度值，但是这个后面没有用到
				float Dis = dot(refractionColor.rgb, float3(0.299, 0.587, 0.114));
				//根据高光的mask贴图，决定读取折射还是反射
				refractionColor = lerp(refractionColor, reflectionColor, tex2D(DifuseMask, i.uv * DifuseMask_ST.xy + DifuseMask_ST.zw));

				float4 Fin = lerp(reflectionColor,  refractionColor * (1.0 - reflectionRate), 1 - reflectionColor);

				Fin = refractionColor + reflectionColor;
				//tongMap
				Fin = ToneMap(Fin, PostExposure, _Disaturate, _Max, _Min, Contrast, 1);
				
				if (Fin.r > 1) {
					Fin.rgb = Fin.rgb * 2;
				}

				if (Fin.r > 1) {
					Fin.rgb = Fin.rgb * 2;
				}

				if (Fin.r > 1) {
					Fin.rgb = Fin.rgb * 2;
				}				

				if ((Fin.r + Fin.g + Fin.b) > 2.9) {
			//		Fin.rgb =  5;
				}

				return Fin;
			}

			ENDCG
		}
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"


			float4 CentrePivotDiamond;
			float4 CentrePivotDiamond2;
			float CentreIntensity;
			float4x4 MatrixWorldToObject;
			float4x4 MatrixWorldToObject2;
			float4 CentreModel;

			struct v2f {
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				float4 pos = v.vertex;

				pos.xyz = lerp(pos.xyz, (pos.xyz - CentreModel.xyz), CentreIntensity);

				o.pos = UnityObjectToClipPos(pos);



				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					o.pos = UnityObjectToClipPos(pos);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}

	}

	FallBack "Diffuse"
}