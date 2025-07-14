Shader "Custom/SDFRaymarchURPWithDepth"
{
  Properties {
    _Depth ("Depth", Range(0.0, 1.0)) = 1.0
  }

  SubShader {
    Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
    LOD 100

    Pass {
      Name "ForwardLit"
      Tags { "LightMode" = "UniversalForward" }

      ZWrite On

      HLSLPROGRAM
      #pragma vertex Vert
      #pragma fragment Frag
      #pragma target 4.5

      // URP Lighting Keywords
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
      #pragma multi_compile _ _SHADOWS_SOFT

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

      // Your SDF include
      #include "Implicit Surfaces.cginc"

      struct Attributes {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct Varyings {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      Varyings Vert (Attributes input) {
        Varyings output;
        // output.positionCS = TransformObjectToHClip(input.positionOS);
        output.positionCS = input.positionOS;
        output.uv = input.uv;
        return output;
      }

      float _Depth;

      struct FragmentOutput {
        float4 color : SV_Target;
        float depth : SV_Depth;
      };

      // Utility function: converts linear depth to raw 0-1 depth buffer value
      float ComputeNormalizedDeviceCoordinatesZ(float viewZ) {
        return (viewZ * _ZBufferParams.z + _ZBufferParams.w);
      }

      float RawDepthFromViewZ(float viewZ) {
        float n = _ProjectionParams.y;
        float f = _ProjectionParams.z;
        return f / (f - n) - (f * n) / ((f - n) * viewZ);
      }

      FragmentOutput Frag (Varyings input) {
        FragmentOutput o;

        float3 camPos = _WorldSpaceCameraPos;
        float4x4 invVP = UNITY_MATRIX_I_VP;

        float value, rawDepth;
        float3 normal;

        RenderSDF_float(input.uv, camPos, invVP, value, normal, rawDepth);

        if (value < 0.5) discard;

        float3 worldPos = camPos + normalize(normal) * rawDepth;
        float3 lightDir = normalize(_MainLightPosition.xyz);
        float lightAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(worldPos));

        float NdotL = max(0, dot(normal, lightDir));
        float3 color = float3(1, 0.75, 0.4) * NdotL * lightAtten;

        o.color = float4(color, 1.0);

        o.depth = rawDepth;

        return o;
      }

      ENDHLSL
    }
  }
  FallBack "Hidden/InternalErrorShader"
}
