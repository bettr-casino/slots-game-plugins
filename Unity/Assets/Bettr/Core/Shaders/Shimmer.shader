Shader "Bettr/Shimmer" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _ShimmerColor ("Shimmer Color", Color) = (1, 1, 1, 1)
        _Speed ("Shimmer Speed", Float) = 0.5
        _Intensity ("Shimmer Intensity", Float) = 0.5
    }
    SubShader {
        Tags { "RenderType" = "Transparent" }
        LOD 100

        Pass {
            ZWrite Off  // Disable depth writing for transparency
            Blend SrcAlpha OneMinusSrcAlpha  // Enable alpha blending

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            sampler2D _MainTex;
            float4 _ShimmerColor;
            float _Speed;
            float _Intensity;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                // Shimmer from top to bottom
                float shimmer = sin(_Time.y * _Speed + i.uv.y * 10.0) * 0.5 + 0.5;

                // Sample the texture
                float4 texColor = tex2D(_MainTex, i.uv);

                // Apply shimmer effect with transparency
                float4 shimmerEffect = _ShimmerColor * shimmer * _Intensity;

                // Combine texture and shimmer, preserving transparency
                return float4(texColor.rgb + shimmerEffect.rgb, texColor.a);
            }
            ENDCG
        }
    }
}
