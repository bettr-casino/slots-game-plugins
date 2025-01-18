Shader "Bettr/SymbolStencil"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha", Range(0,1)) = 1.0
        _StencilRef ("Stencil Ref", Float) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Geometry+2" }

        Pass
        {
            Stencil
            {
                Ref [_StencilRef]    // Match the reference value of the corresponding view rect
                Comp Equal           // Render only where stencil buffer equals Ref
                Pass Keep            // Keep the current stencil buffer value
                Fail Keep            // Do not modify stencil buffer on fail
            }

            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Alpha;

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
                o.pos = UnityObjectToClipPos(v.vertex); // Transform vertex to clip space
                o.uv = v.texcoord;                     // Pass UV coordinates
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                fixed4 tex = tex2D(_MainTex, i.uv); // Sample the texture
                tex.a *= _Alpha;                    // Apply alpha transparency
                return tex * _Color;                // Return the final color
            }
            ENDCG
        }
    }
}
