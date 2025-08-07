Shader "Grass/Rendering"
{
  Properties
  {
    [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
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
      // Cull Off

      HLSLPROGRAM
      #pragma multi_compile_instancing
      #pragma multi_compile_shadowcaster
      #pragma instancing_options renderingLayer

      #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
      #pragma shader_feature_local_fragment _SPECULAR_SETUP

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


      #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
      #pragma multi_compile _ SHADOWS_SHADOWMASK
      #pragma multi_compile _ DIRLIGHTMAP_COMBINED
      #pragma multi_compile _ LIGHTMAP_ON
      #pragma multi_compile _ DYNAMICLIGHTMAP_ON
      #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
      #pragma multi_compile_fog
      #pragma multi_compile_fragment _ DEBUG_DISPLAY

      #pragma vertex vert
      // #pragma fragment frag
      #pragma fragment LitPassFragment

      // Very important for specular
      #define _SPECULAR_COLOR
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

      // Needed for proper indirect drawing (though I cannot see why so far)
      #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
      #include "UnityIndirect.cginc"

      struct GrassInstance
      {
        float3 position;
        float scale;
      };

      StructuredBuffer<GrassInstance> GrassInstances;

      /*
      This is an excellent resource for indirect rendering
      https : //raw.githubusercontent.com / Shitakami / Unity - URP - RenderMehsIndirect / refs / heads / main / URP - GPUInstancing - Sample / Assets / Shaders / ForRenderMeshIndirect.shader
      This is a nice resource for elementary custom URP shaders
      https://gist.githubusercontent.com/NedMakesGames/0fc3cc299443d7efe81f25486c7178ec/raw/c761793e411c93a5add2a9a54003d290cd042223/MyLit.shader
      */
      Varyings vert (Attributes input, uint instanceID : SV_INSTANCEID)
      {
        const float BLADE_HEIGHT = 1;

        InitIndirectDrawArgs(0);
        Varyings varyings = (Varyings)0;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, varyings);
        instanceID = GetIndirectInstanceID(instanceID);
        float3 instancePos = GrassInstances[instanceID].position;
        float4 worldPos = float4(instancePos + input.positionOS.xyz, 1);
        varyings.positionWS = worldPos;
        varyings.positionCS = mul(UNITY_MATRIX_VP, worldPos);
        varyings.normalWS = GetVertexNormalInputs(input.normalOS).normalWS;
        varyings.uv = input.texcoord;
        return varyings;
      }

      // n.b. this shader is not currently used ( LitPassFragment from URP is ) but I'm keeping this code here
      // because it could be useful eventually if we need to write more custom code into the fragment.
      half4 frag (Varyings varyings) : SV_Target
      {
        InputData inputData = (InputData)0;
        inputData.positionWS = varyings.positionWS;
        inputData.normalWS = normalize(varyings.normalWS);
        inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(varyings.positionWS);
        inputData.shadowCoord = TransformWorldToShadowCoord(varyings.positionWS);

        SurfaceData surfaceData = (SurfaceData)0;
        surfaceData.albedo = _BaseColor.rgb;
        surfaceData.alpha = 1;
        surfaceData.specular = half3(1, 1, 1); // this is a color
        surfaceData.smoothness = _Smoothness;
        return UniversalFragmentPBR(inputData, surfaceData);
      }

      ENDHLSL
    }
  }
}