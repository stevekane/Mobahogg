Shader "Grass/Rendering"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
  }
  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
    }
    Pass
    {
      Name "ForwardLit"
      Tags
      {
        "LightMode" = "UniversalForward"
      }

      HLSLPROGRAM
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_instancing
      #pragma multi_compile_shadowcaster
      #pragma instancing_options renderingLayer

      struct Attributes
      {
        float4 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float2 uv : TEXCOORD0;
      };

      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
        float3 normalWS : TEXCOORD1;
      };

      struct GrassInstance
      {
        float3 position;
        float scale;
      };

      StructuredBuffer<GrassInstance> GrassInstances;
      float4 _Color;

      Varyings vert (Attributes input, uint instanceID : SV_INSTANCEID)
      {
        Varyings varyings;
        float3 instancePos = GrassInstances[instanceID].position;
        varyings.positionCS = float4(instancePos + input.positionOS, 1.0);
        varyings.positionCS = mul(UNITY_MATRIX_VP, varyings.positionCS);
        varyings.normalWS = GetVertexNormalInputs(input.normalOS).normalWS;
        varyings.uv = input.uv;
        return varyings;
      }

      half4 frag (Varyings varyings) : SV_Target
      {
        InputData inputData = (InputData)0;
        inputData.normalWS = normalize(varyings.normalWS);
        SurfaceData surfaceData = (SurfaceData)0;
        surfaceData.albedo = _Color.rgb;
        surfaceData.alpha = 1;
        // works
        // return float4(1, 0, 0, 1);
        // works somewhat plausibly
        // return float4(inputData.normalWS, 1);
        return UniversalFragmentBlinnPhong(inputData, surfaceData);
      }

      ENDHLSL
    }
  }
}