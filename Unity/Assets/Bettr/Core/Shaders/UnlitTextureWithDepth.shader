Shader "Bettr/UnlitTextureWithDepth" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _UseTransparency ("Use Transparency", Float) = 1
    }
    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass {
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _USETRANSPARENCY_ON

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _UseTransparency;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                #ifndef _USETRANSPARENCY_ON
                    col.a = 1;
                #endif
                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
}
