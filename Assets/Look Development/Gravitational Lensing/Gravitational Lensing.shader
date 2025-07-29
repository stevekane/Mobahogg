Shader "Tests/Gravitational Lensing"
{
  Properties
  {
    _Background ("Background", 2D) = "white" {}
    _Scale ("Scale", Range(0, 100)) = 1
  }
  SubShader {
    Pass {
      Name "Gravitational Lensing"

      Cull Off
      Blend Off
      ZTest Off
      ZWrite On

      HLSLPROGRAM
      #pragma vertex Vert
      #pragma fragment Frag
      #pragma target 4.5

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Assets/Shaders/Includes/Full Screen.hlsl"

      TEXTURE2D_X(_Background);
      SAMPLER(sampler_Background);
      float4 _Background_TexelSize;
      float _Scale;

      struct Attributes {
        uint vertexID : SV_VertexID;
      };

      struct Varyings {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      struct FragmentOutput {
        float4 color : SV_TARGET;
      };

      Varyings Vert (Attributes input) {
        Varyings output;
        FullScreenQuadFromVertexIDs(input.vertexID, output.uv, output.positionCS);
        #ifdef UNITY_UV_STARTS_AT_TOP
        output.uv.y = 1.0 - output.uv.y;
        #endif
        return output;
      }

      float4 Frag (Varyings input) : SV_TARGET {
        float2 bh = (.5 * float2(_SinTime.w, _CosTime.z) + float2(1,1)) / float2(2,2);
        float2 delta = bh-input.uv;
        float2 direction = normalize(delta);
        float d = length(delta);
        // inside the blackhole
        if (d < .1) return float4(0,0,0,1);
        if (d >= .1 && d < .105) return float4(10, 10, 10, 1);
        float k = 1-smoothstep(.1, .2, d);
        float2 uv = input.uv + k * _Scale * direction;
        float c = SAMPLE_TEXTURE2D_X(_Background, sampler_Background, uv).r;
        return float4(c * k, c * .5, c * .2, 1);
      }
      ENDHLSL
    }
  }
}