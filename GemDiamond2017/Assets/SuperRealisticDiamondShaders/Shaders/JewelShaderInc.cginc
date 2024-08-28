// Upgrade NOTE: replaced 'UNITY_PASS_TEXCUBE(unity_SpecCube1)' with 'UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0)'


float Test_;

struct appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
    float4 Color : COLOR;
    uint id : SV_VertexID;
    float4 tangent : TANGENT;
};

float FresnelDispersionPower;
float FresnelDispersionScale;
float ColorByDepth;

float CentreIntensity;
float4x4 MatrixWorldToObject;
float4x4 MatrixWorldToObject2;
float4 CentreModel;
float lightEstimation2;
float MipLevel;

samplerCUBE _Environment;
half4 _Environment_HDR;
float FixedlightEstimation;
float ColorIntensity;
struct v2f 
{
	float2 uv : TEXCOORD0; 
    float3 WorldBitangent : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
	float4 vertex : SV_POSITION; 
	float3 Pos : TEXCOORD3; 
    float3 Pos2 : TEXCOORD4;
	float3 Normal : NORMAL;
    float4 Color : COLOR;
    uint id : TEXCOORD5;
    float3 worldPos : TEXCOORD6;
    float4 tangent : TEXCOORD7;
};
			
// vertex shader
v2f vert (appdata v)
{
	v2f o;    

    UNITY_INITIALIZE_OUTPUT(v2f, o);
   
    //构建世界切线，法线，次法线
    float3 _worldTangent = UnityObjectToWorldDir(v.tangent);
    o.tangent.xyz = _worldTangent;
    float3 _worldNormal = UnityObjectToWorldNormal(v.normal);
    o.WorldNormal.xyz = _worldNormal;
    float _vertexTangentSign = v.tangent.w * unity_WorldTransformParams.w;
    float3 _worldBitangent = cross(_worldNormal, _worldTangent) * _vertexTangentSign;
    o.WorldBitangent.xyz = _worldBitangent;
    
    float4 pos = v.vertex;
	//得到了一个相对于中心点的一个本地坐标
	//其实是得到相对于轴心点的位置，因为后边的摄像机的位置也是根据模型的轴心点来的。
	//摄像机变换到模型本地的矩阵中和这里都减去了CentreModel，是不是也可以都不减去呢
    pos.xyz = (pos.xyz - CentreModel.xyz);
    
    o.vertex = UnityObjectToClipPos(pos);
    
    float3 cameraLocalPos;
    
	//获取摄像机在钻石模型空间中的位置
    cameraLocalPos = mul(MatrixWorldToObject, float4(_WorldSpaceCameraPos, 1));
    
    
    
    //Pos2就是摄像机在钻石模型空间中的位置
    o.Pos2 = cameraLocalPos;
    o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	//Pos就是相对于中心点的本地坐标
    o.Pos = float4(pos.xyz, 1);
				o.Normal = v.normal;
    o.Color = v.Color;
    o.id = v.id;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	return o;
}


float Dispersion;

float DispersionLimitedAngle;

float DispersionR;
float DispersionG;
float DispersionB;
float Brightness;
float Power;



float DispersionIntensity;
sampler2D _ShapeTex;
float _Scale;
float TotalInternalReflection;
int _SizeX;
int _SizeY;
int _PlaneCount;
int _MaxReflection;

samplerCUBE ReflectionCube;
		//	samplerCUBE _Environment;
// half4 _Environment_HDR;
float _RefractiveIndex;

float _RefractiveIndex_;

float _BaseReflection;
		
#define MAX_REFLECTION (10)

float random(float2 st)
{
    float r = frac(sin(dot(st.xy,
					float2(12.9898, 78.233)))
					* 43758.5453123);
    return r * clamp(pow(distance(r, 0.6), 2.5) * 100, 0, 1);
}



float CalcReflectionRate(float3 normal, float3 ray, float baseReflection, float borderDot)
{
    float normalizedDot = clamp((abs(dot(normal, ray)) - borderDot) / (1.0 - borderDot), 0.0, 1.0);
    
	return baseReflection + (1.0-baseReflection)*pow(1.0-normalizedDot, 5);
}


half rgb2hsv(half3 c)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
    half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return abs(q.z + (q.w - q.y) / (6.0 * d + e));
}

float Remap(float value, float min1, float max1, float min2, float max2)
{
    return (min2 + (value - min1) * (max2 - min2) / (max1 - min1));
}

//这个函数返回的是_ShapeTex中存储的法线信息
float4 GetUnpackedPlaneByIndex(uint index)
{
	int x_index = index % _SizeX;
	int y_index = index / _SizeX;

	float ustride = 1.0 / _SizeX;
	float vstride = 1.0 / _SizeY;

	float2 uv = float2((0.5+x_index)*ustride, (0.5+y_index)*vstride);

	float4 packedPlane = tex2D(_ShapeTex, uv);

#if !defined(UNITY_COLORSPACE_GAMMA)
	packedPlane.xyz = LinearToGammaSpace(packedPlane.xyz);
#endif
	//color*2 - 1
	float3 normal = packedPlane.xyz*2 - float3(1,1,1); // смена диапозона

	return float4(normal, packedPlane.w*_Scale);//a的数值表示：顶点的本地坐标在法线方向上的投影长度
}

//--rayStart 相对于几何中心点的本地坐标
//--rayDirection 折射方向
float CheckCollideRayWithPlane(float3 rayStart, float3 rayNormalized, float4 normalTriangle) // plane - normal.xyz и normal.w - distance
{
	//计算折射方向和法线方向的点积,也就是折射方向在法线方向上的投影长度
    float dp = dot(rayNormalized, normalTriangle.xyz);

	if( dp < 0 )
	{
		return -1;
	}
	else
	{
		//获取顶点在法线方向上投影的长度 - 相对于几何中心点的本地坐标在法线方向上投影的长度
        float distanceNormalized = normalTriangle.w - dot(rayStart.xyz, normalTriangle.xyz);

		if( distanceNormalized < 0 )
		{
			return -1;
		}

		//
		return distanceNormalized / dp;
	}


	return -1;
}

/*
--Pos 相对于几何中心点的本地坐标
--PassCount
--rayNormalized 摄像机指向顶点的向量
--TriangleNormal 三角面的发现
--startSideRelativeRefraction 起始侧的相对折射
--reflectionRate 反射率
--reflectionRate2 反射率2
--reflection 反射方向
--refraction 折射方向
--HorizontalElementSquared 
*/
void CollideRayWithPlane(float3 Pos, float PassCount, float3 rayNormalized, float4 TriangleNormal, 
						float startSideRelativeRefraction, out float reflectionRate, 
						out float reflectionRate2, out float3 reflection, out float3 refraction, out float HorizontalElementSquared)
{
	//垂直于三角面的射线
    float3 rayVertical = dot(TriangleNormal.xyz, rayNormalized) * TriangleNormal.xyz;
	//反射公式
	reflection = rayNormalized - rayVertical*2.0;

	//水平方向反射
	float3 rayHorizontal = rayNormalized - rayVertical;
	//水平折射方向，折射方向也没变啊，乘的是一个数，方向不会变化
	float3 refractHorizontal = rayHorizontal * startSideRelativeRefraction ;
	//水平折射方向的平方
	float horizontalElementSquared = dot(refractHorizontal, refractHorizontal);
				
	/**/
    float borderDot = 0;

    
	if( startSideRelativeRefraction > 1.0 )
	{
		borderDot = sqrt(1.0-1.0f/(startSideRelativeRefraction*startSideRelativeRefraction));
	}
	else
	{
		borderDot = 0.0;
	}     
    
    HorizontalElementSquared = 0;
    
    HorizontalElementSquared = horizontalElementSquared /3;
    if (horizontalElementSquared >= TotalInternalReflection)  
	{
        HorizontalElementSquared = 0;
        
        
		reflectionRate = 1.0;
        reflectionRate2 = 1.0;
        refraction = TriangleNormal.xyz;

		return;
	}				
	//垂直大小的平方 = 1 - 水平大小的平方
	float verticalSizeSquared = 1-horizontalElementSquared;
	//垂直方向上的折射方向
	float3 refractVertical = rayVertical * sqrt( verticalSizeSquared / dot(rayVertical, rayVertical));
    
	//折射方向等于水平的加上垂直的
	refraction = refractHorizontal + refractVertical;
    
    reflectionRate = CalcReflectionRate(rayNormalized, TriangleNormal.xyz, _BaseReflection * PassCount, borderDot);

    reflectionRate2 = CalcReflectionRate(rayNormalized, TriangleNormal.xyz, _BaseReflection * PassCount, borderDot);    
	return;
}

float3 CalcColorCoefByDistance(float distance,float4 Color)
{

    return lerp(pow(max(Color.xyz, 0.01), distance * Color.w), Color.rgb, ColorByDepth);
}

float4 SampleEnvironment(float3 rayLocal)
{
	float3 rayWorld = mul(unity_ObjectToWorld, float4(rayLocal, 0));

	rayWorld = normalize(rayWorld);

    
#if _CUBEMAPMODE_CUBEMAP 
    float4 tex = texCUBElod(_Environment, float4(rayWorld,MipLevel));
	return float4(DecodeHDR(tex, _Environment_HDR), 1);
    
#endif 
    
#if _CUBEMAPMODE_REFLECTIONPROBE    
    float4 tex =   UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0,rayWorld, MipLevel);
    return float4(DecodeHDR(tex, unity_SpecCube0_HDR), 1);
#endif
}

//--rayStart 相对于几何中心点的本地坐标
//--rayDirection 折射方向
void CheckCollideRayWithAllPlanes(float3 rayStart, float3 rayDirection, out float4 hitPlane, out float hitTime)
{
	hitTime=1000000.0;
	hitPlane=float4(1,0,0,1);
	//_PlaneCount是执行CalculateMesh的结果
//	[unroll(20)]
    for(int i=0; i<_PlaneCount; ++i)
	{
		//plane是获取ShapeTex中存储的法线信息
		float4 plane = GetUnpackedPlaneByIndex(i);
		float tmpTime = CheckCollideRayWithPlane(rayStart, rayDirection, plane);

		if(tmpTime >= -0.001 && tmpTime<hitTime)
		{
			hitTime = tmpTime;
			hitPlane = plane;
		}
	}
}
/*
--rayStart 相对于几何中心点的本地坐标
--rayDirection 折射方向
--refractiveIndex 折射率
--MaxReflection 最大反射
--_Color 外部传入的颜色
--lighttransmission 光透射
*/
float4 GetColorByRay(float3 rayStart, float3 rayDirection, float refractiveIndex, int MaxReflection, float4 Color, float lighttransmission)
{
	float3 tmpRayStart = rayStart;
	float3 tmpRayDirection = rayDirection;

	//定义反射和折射数组，最大长度是10
	float reflectionRates[MAX_REFLECTION];
    float reflectionRates2[MAX_REFLECTION];
	float4 refractionColors[MAX_REFLECTION];
    float4 refractionColors2[MAX_REFLECTION];
    float4 refractionColors3[MAX_REFLECTION];
	float4 depthColors[MAX_REFLECTION];

    int loopCount = min(MAX_REFLECTION, _MaxReflection);//_MaxReflection在脚本里设置的最大反射数

	int badRay = 0;

  //  [unroll(10)]
    for( int i = 0; i<loopCount; ++i )
	{
		float hitTime=1000000.0;
		float4 hitPlane=float4(1,0,0,1);
		//--tmpRayStart 相对于几何中心点的本地坐标
		//--tmpRayDirection 折射方向
		//--hitPlane 返回hitPlane
		//--hitTime 返回hitTime
		CheckCollideRayWithAllPlanes(tmpRayStart, tmpRayDirection, hitPlane, hitTime);

		if (hitTime < 0.0)
		{
			badRay = 1;
		}
										
		float3 rayEnd = tmpRayStart + tmpRayDirection*hitTime;
								
		float reflectionRate;
        float reflectionRate2;
		float3 reflectionRay;
		float3 refractionRay;
        float PlaneNull;

        float i_Pass = i;
        
        if (i_Pass >= 2)
        {
            i_Pass = 0;

        }
        
        if (i_Pass < 2)
        {
            i_Pass = 1;

        }
        
        
        CollideRayWithPlane(rayStart, i_Pass, tmpRayDirection, hitPlane, refractiveIndex, reflectionRate,reflectionRate2, reflectionRay, refractionRay, PlaneNull);
		
        reflectionRates[i] = reflectionRate;
        
        reflectionRates2[i] = reflectionRate2;
        
        float Disp = pow(Dispersion , 2);
        
        float dispPow =  Dispersion * 0.4;
 
        float depth2 = Remap(i, 0, loopCount, 0, 1);        
        
        depth2 = clamp(depth2, 0.0001, 1);
        
        depth2 = 1;
 
        //世界空间下，由此点到摄像机的方向（参数应该是世界坐标，但是rayStart这个是本地坐标啊）
        float3 _worldViewDir = UnityWorldSpaceViewDir(rayStart.xyz);
        _worldViewDir = normalize(_worldViewDir);//归一化

        float fresnelNdotV5 = dot(tmpRayStart, _worldViewDir);
        float fresnelNode5 = (FresnelDispersionScale * pow(1.0 - fresnelNdotV5, FresnelDispersionPower));
        
        fresnelNode5 = 1;//这里强制为1了；上面计算的作废，也就是没有用到
        
        DispersionR = DispersionR * Dispersion * fresnelNode5;
        DispersionG = DispersionG * Dispersion * fresnelNode5;
        DispersionB = DispersionB * Dispersion * fresnelNode5;
        
        
        float3 DispersionRay_r = lerp(refractionRay, lerp(rayEnd, refractionRay,2), DispersionR * PlaneNull);

        float3 DispersionRay_g = lerp(refractionRay, lerp(rayEnd, refractionRay, 2), DispersionG * PlaneNull);

        float3 DispersionRay_b = lerp(refractionRay, lerp(rayEnd, refractionRay, 2), DispersionB * PlaneNull);

        float Depth_ = depthColors[i];
        
        Depth_ = Remap(Depth_, 0.997, 0.999, 1, 0);
        
        
        refractionColors3[i] = SampleEnvironment(refractionRay);
        
        refractionColors2[i] = 1;
        
        refractionColors2[i].r = SampleEnvironment(DispersionRay_r).r;
        refractionColors2[i].g = SampleEnvironment(DispersionRay_g).g;
        refractionColors2[i].b = SampleEnvironment(DispersionRay_b).b;
        
        Color.rgb = lerp(1, Color, ColorIntensity).rgb;
        
        depthColors[i] = float4(CalcColorCoefByDistance(hitTime, lerp(Color, 1, lerp(0, (refractionColors3[i].r + refractionColors3[i].g + refractionColors3[i].b) / 2, lighttransmission))), 1);

        refractionColors2[i] = clamp(lerp(refractionColors3[i], refractionColors2[i], DispersionIntensity),0,1); 

        float CLR = refractionRay.x;
        
        if (CLR < 0)
        {
            CLR = CLR * -1;

        }
        
        refractionColors[i] = SampleEnvironment(refractionRay);        
        
        float DispRandom = pow(random(hitPlane.xy),0.1);
        float3 DirDisp = clamp(tmpRayStart.rgb,-1,1);					

        if (i == loopCount - 1)
        {
            reflectionRates[i] = 0.0;
            reflectionRates2[i] = 0.0;
        }

        tmpRayStart = tmpRayStart + tmpRayDirection * hitTime;
        tmpRayDirection = reflectionRay;
    }
				
    float4 tmpReflectionColor = float4(0, 0, 0, 0);
				
    for (int j = loopCount - 1; j >= 0; --j)
    {
        float4 refractionColors_;
     
        tmpReflectionColor = lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j];        
        tmpReflectionColor = pow(tmpReflectionColor * Brightness, Power);        
    }
				
	if (badRay > 0)
	{
		return float4(1, 0, 0, 1);
	}

	return tmpReflectionColor;
}


float4 CalculateContrast(float contrastValue, float4 colorTarget)
{
    float t = 0.5 * (1.0 - contrastValue);
    return mul(float4x4(contrastValue, 0, 0, t, 0, contrastValue, 0, t, 0, 0, contrastValue, t, 0, 0, 0, 1), colorTarget);
}

float4 ToneMap(float4 MainColor, float brightness, float Disaturate, float _max, float _min, float contrast, float Satur)
{				
    fixed4 output = MainColor;
			//	output = output * brightness;
    output = output * brightness;
    output = CalculateContrast(contrast, output);

    float4 disatur = dot(output, float3(0.299, 0.587, 0.114)); // Desaturate
    output = lerp(output, disatur, clamp(pow(((output.x + output.y + output.z) / 3) * Disaturate, 1.3), 0, 1));
    output.x = clamp(Remap(output.x, 0, 1, _min, lerp(_max, 1, 0.5)), 0, 1.5);
    output.y = clamp(Remap(output.y, 0, 1, _min, lerp(_max, 1, 0.5)), 0, 1.5);
    output.z = clamp(Remap(output.z, 0, 1, _min, lerp(_max, 1, 0.5)), 0, 1.5);

    output = pow(output, contrast);

    output = lerp(clamp(output, 0, _max), output, pow(_max, 4));

    output = lerp(smoothstep(output, -0.1, 0.25), output, (1 - distance(1, _max) * 2));
				
    output = lerp(dot(output, float3(0.299, 0.587, 0.114)), output, Satur);

    output = output * lerp(brightness, 1, 0.75);

    return output;
}