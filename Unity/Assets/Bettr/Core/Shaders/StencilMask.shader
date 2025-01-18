Shader "Bettr/StencilMask"
{
    SubShader
    {
        Tags { "Queue" = "Geometry+1" }

        Pass
        {
            Stencil
            {
                Ref [_StencilRef]    // Unique reference value for each view rect
                Comp Always          // Always pass the stencil test
                Pass Replace         // Replace the stencil buffer value with Ref
            }

            ColorMask 0             // Do not render any color (invisible object)
            ZWrite Off              // Do not write to the depth buffer
        }
    }
}
