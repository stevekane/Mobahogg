Shader "Tests/Gravitational Lensing"
{
  Properties
  {
    _Background ("Background", 2D) = "white" {}
    _EventHorizon ("Event Horizon", Range(0, 1000)) = 60
    _PhotonSphereThickness ("Photon Sphere Thickness", Range(0, 100)) = 2
    _EinsteinRing ("Einstein Ring", Range(0, 1000)) = 90
    _Scale ("Scale", Range(0, 1000)) = 1
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
      #include "Assets/Shaders/Includes/Noise.hlsl"

      TEXTURE2D_X(_Background);
      SAMPLER(sampler_Background);
      float4 _Background_TexelSize;
      float _Scale;
      float _EventHorizon;
      float _PhotonSphereThickness;
      float _EinsteinRing;

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

      float2 UV_to_ScreenTexel(float2 uv) {
        return floor(uv * _ScreenParams.xy) + 0.5;
      }

      float2 ScreenTexel_to_UV(float2 texel) {
        return (texel + 0.5) / _ScreenParams.xy;
      }

      float4 Frag (Varyings input) : SV_TARGET {
        float2 bh_uv = (.5 * float2(_SinTime.w, _CosTime.z) + float2(1,1)) / float2(2,2);
        float2 bh = UV_to_ScreenTexel(bh_uv);
        float2 p = UV_to_ScreenTexel(input.uv) ;
        // do calculations in texel_space
        float2 delta = bh-p;
        float2 direction = normalize(delta);
        float d = length(delta);
        // inside the blackhole
        const float DISTORTION = _EventHorizon + _PhotonSphereThickness;
        if (d < _EventHorizon) return float4(0,0,0,1);
        if (d >= _EventHorizon && d < DISTORTION) return float4(10*float3(1, .5, .2), 1);
        float k = 1-smoothstep(DISTORTION, _EinsteinRing, d);
        float2 p1 = p + k * _Scale * direction;
        float2 p1_uv = ScreenTexel_to_UV(p1) + k * noise(p / 100);
        float c = SAMPLE_TEXTURE2D_X(_Background, sampler_Background, p1_uv).r;
        return float4(c * k, c * .5, c * .2, 1);
      }
      ENDHLSL
    }
  }
}