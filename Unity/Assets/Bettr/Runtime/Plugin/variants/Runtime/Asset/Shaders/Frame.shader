// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Bettr/Frame"
{
	Properties 
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Texture", 2D) = "white" {}

		[Toggle] Alpha_Split("Use AlphaSplit?", Float) = 0
		_AlphaTex ("Texture (AlphaSplit)", 2D) = "white" {}
		
		_Stencil ("Stencil Ref", Float) = 0
		_StencilComp ("Stencil Comparison", Float) = 8
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Lighting Off
		Alphatest Greater 0
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		
		Pass
		{
CGPROGRAM
#pragma multi_compile ALPHA_SPLIT_OFF ALPHA_SPLIT_ON
#pragma vertex vert 
#pragma fragment frag

#include "UnityCG.cginc"

fixed4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
#ifdef ALPHA_SPLIT_ON
sampler2D _AlphaTex;
#endif

struct appdata 
{
	float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
};

struct v2f
{
 	float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

v2f vert (appdata v) 
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    return o;
}

fixed4 frag (v2f i) : COLOR
{
	float4 tex = tex2D(_MainTex, i.uv);
	#ifdef ALPHA_SPLIT_ON
	tex.a = tex2D(_AlphaTex, i.uv).g;
	#endif
	return tex * _Color;
}

ENDCG
Stencil
{
	Ref [_Stencil]
	Comp [_StencilComp]
	Pass Keep
}
		}
	}
}
