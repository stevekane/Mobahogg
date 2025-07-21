Shader "Custom/FullScreenTest"
{
    SubShader
    {
        Pass
        {
            Name "FullScreenTest"

            ZTest Always
            ZWrite On
            Blend Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment FullscreenFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Full Screen Utils.cginc"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings FullscreenVert(Attributes input)
            {
                Varyings output;
                FullScreenQuadFromVertexIDs(input.vertexID, output.uv, output.positionCS);
                return output;
            }

            float FullscreenFrag(Varyings input) : SV_DEPTH
            {
                return input.uv.x;
            }
            ENDHLSL
        }
    }
}
