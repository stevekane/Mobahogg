Shader "Grass/URP Blinn Phong"
{
  Properties
  {
    _Color ("Color", Color) = (1, 1, 1, 1)
    _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
  }
  SubShader
  {
    Tags
    {
      "RenderPipeline" = "UniversalPipeline"
    }
    LOD 300 // no idea what this does... should find out at some point
    Pass
    {
      Name "ForwardLit"
      Tags
      {
        "LightMode" = "UniversalForward"
      }

      HLSLPROGRAM
      #define _SPECULAR_COLOR

      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
      #pragma multi_compile_fragment _ _SHADOWS_SOFT
      #pragma multi_compile _ _FORWARD_PLUS

      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

      float4 _Color;

      Varyings vert (Attributes input)
      {
        Varyings varyings = (Varyings)0;
        VertexPositionInputs vpi = GetVertexPositionInputs(input.positionOS.xyz);
        varyings.positionWS = vpi.positionWS;
        varyings.positionCS = vpi.positionCS;
        varyings.normalWS = GetVertexNormalInputs(input.normalOS).normalWS;
        varyings.uv = input.texcoord;
        return varyings;
      }

      half4 frag (Varyings varyings) : SV_Target
      {
        InputData inputData = (InputData)0;
        inputData.positionWS = varyings.positionWS;
        inputData.normalWS = normalize(varyings.normalWS);
        inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(varyings.positionWS);
        inputData.shadowCoord = TransformWorldToShadowCoord(varyings.positionWS);

        SurfaceData surfaceData = (SurfaceData)0;
        surfaceData.albedo = _Color.rgb;
        surfaceData.alpha = 1;
        surfaceData.smoothness = _Smoothness;
        surfaceData.specular = half3(1,1,1); // this is actually a color
        return UniversalFragmentPBR(inputData, surfaceData);
      }

      ENDHLSL
    }
    Pass {
      Name "ShadowCaster"
      Tags
      {
        "LightMode" = "ShadowCaster"
      }
      ColorMask 0
    }
  }
}