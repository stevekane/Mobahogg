Shader "Grass/Rendering"
{
  Properties
  {
    _Color ("Color", Color) = (1, 1, 1, 1)
    _Smoothness ("Smoothness", Range(0.0, 1.0)) = 1.0
  }
  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
      "UniversalMaterialType" = "Lit"
      "IgnoreProjector" = "True"
    }
    Pass
    {
      Name "ForwardLit"
      Tags
      {
        "LightMode" = "UniversalForward"
      }

      HLSLPROGRAM
      // Very important for specular. no idea why
      #define _SPECULAR_COLOR
      #pragma vertex vert
      #pragma fragment frag
      // #pragma fragment LitPassFragment
      #pragma multi_compile_instancing
      #pragma multi_compile_shadowcaster
      // #pragma instancing_options renderingLayer
      #pragma shader_feature_local _NORMALMAP
      #pragma shader_feature_local _PARALLAXMAP
      #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
      #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
      #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
      #pragma shader_feature_local_fragment _ALPHATEST_ON
      #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
      #pragma shader_feature_local_fragment _EMISSION
      #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
      #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
      #pragma shader_feature_local_fragment _OCCLUSIONMAP

      // #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
      #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
      #pragma shader_feature_local_fragment _SPECULAR_SETUP

      // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -
      // Universal Pipeline keywords
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
      #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
      #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
      #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
      #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
      #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
      #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
      #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
      #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
      #pragma multi_compile_fragment _ _LIGHT_COOKIES
      #pragma multi_compile _ _LIGHT_LAYERS
      #pragma multi_compile _ _FORWARD_PLUS
      // #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
      // #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"


      // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -
      // Unity defined keywords
      #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
      #pragma multi_compile _ SHADOWS_SHADOWMASK
      #pragma multi_compile _ DIRLIGHTMAP_COMBINED
      #pragma multi_compile _ LIGHTMAP_ON
      #pragma multi_compile _ DYNAMICLIGHTMAP_ON
      #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
      #pragma multi_compile_fog
      #pragma multi_compile_fragment _ DEBUG_DISPLAY

      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
      #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs

      struct GrassInstance
      {
        float3 position;
        float scale;
      };

      StructuredBuffer<GrassInstance> GrassInstances;
      float4 _Color;

      Varyings vert (Attributes input, uint instanceID : SV_INSTANCEID)
      {
        Varyings varyings = (Varyings)0;
        float3 instancePos = GrassInstances[instanceID].position;
        float4 worldPos = float4(instancePos + input.positionOS.xyz, 1);
        varyings.positionWS = worldPos;
        varyings.positionCS = mul(UNITY_MATRIX_VP, worldPos);
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
        surfaceData.specular = half3(1,1,1); // this is a color
        surfaceData.smoothness = _Smoothness;
        return UniversalFragmentPBR(inputData, surfaceData);
      }

      ENDHLSL
    }
  }
}