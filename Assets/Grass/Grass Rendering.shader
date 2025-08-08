Shader "Grass/Rendering"
{
  Properties
  {
    [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
    _TipColor ("Tip Color", Color) = (1, 1, 1, 1)
    _Smoothness ("Smoothness", Range(0.0, 1.0)) = 1.0
    _NormalBend ("Normal Bend", Range(0, 1.57079)) = 1.57079
    _BladeCurve ("Blade Curve", Range(0, 3.14159)) = 3.14159
    _ThicknessMultiplier ("Thickness Multiplier", Range(0, 100)) = 10
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
      #pragma fragment frag

      // Very important for specular
      #define _SPECULAR_COLOR
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

      // #define DEBUG_SHOW_NORMALS

      // Needed for proper indirect drawing (though I cannot see why so far)
      #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
      #include "UnityIndirect.cginc"

      struct GrassInstance
      {
        float3 position;
        float scale;
      };

      StructuredBuffer<GrassInstance> GrassInstances;

      float3 RotateAboutOrigin(float3 posOS, float maxRotationRadians, float maxHeight)
      {
        float h = saturate(posOS.y / maxHeight);
        // float w = h * h;
        float w = h;
        float angle = maxRotationRadians * w;
        float s, c;
        sincos(angle, s, c);
        float y = posOS.y;
        float z = posOS.z;
        posOS.y = y * c - z * s;
        posOS.z = y * s + z * c;
        return posOS;
      }

      float3 RotateNormalAboutOrigin(float3 posOS, float3 normalOS, float maxRotationRadians, float maxHeight)
      {
        float h = saturate(posOS.y / maxHeight);
        // float w = h * h;
        float w = h;
        // TODO: What in the christmas fuck. Why does this seem to work better?
        float angle = maxRotationRadians * w * 2.;
        float s, c;
        sincos(angle, s, c);
        float y = normalOS.y;
        float z = normalOS.z;
        normalOS.y = y * c - z * s;
        normalOS.z = y * s + z * c;
        return normalize(normalOS);
      }

      float3 RotateNormalAroundY(float3 normal, float radians)
      {
        float s, c;
        sincos(radians, s, c);
        float x = normal.x;
        float z = normal.z;
        normal.x = x * c + z * s;
        normal.z = - x * s + z * c;
        return normalize(normal);
      }

      float _NormalBend;
      float _BladeCurve;
      float _ThicknessMultiplier;
      float4 _TipColor;
      Varyings vert (Attributes input, uint instanceID : SV_INSTANCEID)
      {
        const float BLADE_HEIGHT = 1;
        const float BLADE_RADIUS = 0.015;

        Varyings varyings = (Varyings)0;

        InitIndirectDrawArgs(0);
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, varyings);
        instanceID = GetIndirectInstanceID(instanceID);

        float3 instancePos = GrassInstances[instanceID].position;
        float3 vertexPosition = input.positionOS.xyz;
        vertexPosition = RotateAboutOrigin(vertexPosition, _BladeCurve, BLADE_HEIGHT);
        vertexPosition.x *= _ThicknessMultiplier;
        float4 worldPos = float4(instancePos + vertexPosition, 1);
        float3 normalOS = input.normalOS;
        normalOS = RotateNormalAroundY(normalOS, clamp(input.positionOS.x / BLADE_RADIUS, -1, 1) * _NormalBend);
        normalOS = RotateNormalAboutOrigin(input.positionOS.xyz, normalOS, _BladeCurve, BLADE_HEIGHT);
        varyings.positionWS = worldPos;
        varyings.positionCS = mul(UNITY_MATRIX_VP, worldPos);
        varyings.normalWS = normalOS;
        varyings.uv = input.texcoord;
        return varyings;
      }

      half4 frag (Varyings varyings) : SV_Target
      {
        UNITY_SETUP_INSTANCE_ID(varyings);
        InputData inputData = (InputData)0;
        inputData.positionWS = varyings.positionWS;
        inputData.normalWS = normalize(varyings.normalWS);
        inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(varyings.positionWS);
        inputData.shadowCoord = TransformWorldToShadowCoord(varyings.positionWS);

        SurfaceData surfaceData = (SurfaceData)0;
        surfaceData.albedo = lerp(_BaseColor.rgb, _TipColor.rgb, varyings.uv.y);
        surfaceData.alpha = 1;
        surfaceData.specular = half3(1, 1, 1);
        surfaceData.smoothness = _Smoothness;
        #ifdef DEBUG_SHOW_NORMALS
        return half4(abs(inputData.normalWS), 1);
        #endif
        return UniversalFragmentPBR(inputData, surfaceData);
      }

      ENDHLSL
    }
  }
}