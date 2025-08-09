Shader "Grass/Rendering"
{
  Properties
  {
    [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
    _TipColor ("Tip Color", Color) = (1, 1, 1, 1)
    _Smoothness ("Smoothness", Range(0.0, 1.0)) = 1.0
    _NormalBend ("Normal Bend", Range(0, 1.57079)) = 1.57079
    _BladeCurve ("Blade Curve", Range(0, 3.14159)) = 3.14159
    _BladeHeight ("Blade Height", Range(0, 3)) = 1
    _BladeRadius ("Blade Radius", Range(0, 1)) = .015
    _LowFrequency ("Low Frequency", Range(0, 10)) = 1
    _HighFrequency ("High Frequency", Range(0, 10)) = 2
    _MaxYRotationLowFrequency ("Max Y Rotation at Low Frequency", Range(0, 360)) = 180
    _MaxYRotationHighFrequency ("Max Y Rotation at High Frequency", Range(0, 360)) = 24
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
      Cull Off

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
      #include "Assets/Shaders/Includes/Bezier.hlsl"

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
        // TODO : What in the christmas fuck. Why does this seem to work better?
        float angle = maxRotationRadians * w * 2.;
        float s, c;
        sincos(angle, s, c);
        float y = normalOS.y;
        float z = normalOS.z;
        normalOS.y = y * c - z * s;
        normalOS.z = y * s + z * c;
        return normalize(normalOS);
      }

      void BendNormalAboutY(inout float3 normal, float radians)
      {
        float s, c;
        sincos(radians, s, c);
        float x = normal.x;
        float z = normal.z;
        normal.x = x * c + z * s;
        normal.z = - x * s + z * c;
        normal = normalize(normal);
      }

      float hash21(float2 p) {
        p = frac(p * float2(123.34, 456.21));
        p += dot(p, p + 78.233);
        return frac(p.x * p.y);
      }

      float noise2D(float2 p) {
        float2 i = floor(p);
        float2 f = frac(p);
        float a = hash21(i);
        float b = hash21(i + float2(1, 0));
        float c = hash21(i + float2(0, 1));
        float d = hash21(i + float2(1, 1));
        float2 u = f * f * (3.0 - 2.0 * f);
        return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y) * 2.0 - 1.0;
      }

      void GrassNoiseRotateY(
      inout float3 p,
      inout float3 n,
      float2 worldXZ,
      float freq,
      float maxAngle)
      {
        float a = noise2D(worldXZ * freq) * maxAngle;
        float s, c;
        sincos(a, s, c);
        p = float3(c * p.x + s * p.z, p.y, - s * p.x + c * p.z);
        n = normalize(float3(c * n.x + s * n.z, n.y, - s * n.x + c * n.z));
      }


      float _NormalBend;
      float _BladeCurve;
      float _BladeRadius;
      float _BladeHeight;
      float4 _TipColor;

      float _LowFrequency;
      float _HighFrequency;
      float _MaxYRotationLowFrequency;
      float _MaxYRotationHighFrequency;

      Varyings vert (Attributes input, uint instanceID : SV_INSTANCEID)
      {
        const float DEG_TO_RAD = 2 * 3.14159 / 360;

        Varyings varyings = (Varyings)0;

        InitIndirectDrawArgs(0);
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, varyings);
        instanceID = GetIndirectInstanceID(instanceID);

        float3 instancePos = GrassInstances[instanceID].position;
        float3 positionOS = input.positionOS.xyz;
        float3 normalOS = input.normalOS;
        float bladeHeight = .75f * _BladeHeight + .25 * _BladeHeight * noise2D(instancePos.xz * .5);
        positionOS = RotateAboutOrigin(positionOS, _BladeCurve, bladeHeight);
        positionOS.x *= _BladeRadius;
        positionOS.y *= bladeHeight;
        BendNormalAboutY(normalOS, clamp(input.positionOS.x / _BladeRadius, - 1, 1) * _NormalBend);
        normalOS = RotateNormalAboutOrigin(input.positionOS.xyz, normalOS, _BladeCurve, bladeHeight);
        GrassNoiseRotateY(positionOS, normalOS, instancePos.xz, _LowFrequency, DEG_TO_RAD * _MaxYRotationLowFrequency);
        GrassNoiseRotateY(positionOS, normalOS, instancePos.xz, _HighFrequency, DEG_TO_RAD * _MaxYRotationHighFrequency);

        float3 worldPos = instancePos + positionOS;
        float4 worldPos4D = float4(worldPos, 1);
        varyings.positionWS = worldPos4D;
        varyings.positionCS = mul(UNITY_MATRIX_VP, worldPos4D);
        varyings.normalWS = normalOS;
        varyings.uv = input.texcoord;
        return varyings;
      }

      half4 frag (Varyings varyings, bool isFrontFace : SV_ISFRONTFACE) : SV_Target
      {
        UNITY_SETUP_INSTANCE_ID(varyings);
        InputData inputData = (InputData)0;
        inputData.positionWS = varyings.positionWS;
        inputData.normalWS = (isFrontFace ? 1 : -1) * normalize(varyings.normalWS);
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