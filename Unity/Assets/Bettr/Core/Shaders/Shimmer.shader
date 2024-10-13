Shader "Bettr/Shimmer" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {} // Main texture
        _ShimmerColor ("Shimmer Color", Color) = (1, 1, 1, 0.5) // Light shimmer color with transparency
        _Speed ("Shimmer Speed", Float) = 0.3   // Reduced speed for smoother movement
        _Intensity ("Shimmer Intensity", Float) = 0.3  // Lowered intensity for subtle effect
    }
    SubShader {
        Tags { "RenderType"="Transparent" } // Enable transparency support
        LOD 100

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha // Proper blending for transparent textures
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // Transform vertex to clip space
                o.uv = v.uv; // Pass UVs to the fragment shader
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                // Create a shimmer effect moving from bottom to top
                float shimmer = sin(_Time.y * _Speed - i.uv.y * 10.0) * 0.5 + 0.5;

                // Sample the texture with transparency
                float4 texColor = tex2D(_MainTex, i.uv);

                // Apply shimmer effect with intensity, supporting transparency
                float4 shimmerEffect = _ShimmerColor * shimmer * _Intensity;

                // Blend the shimmer effect with the original texture
                return lerp(texColor, texColor + shimmerEffect, texColor.a);
            }
            ENDCG
        }
    }
}
