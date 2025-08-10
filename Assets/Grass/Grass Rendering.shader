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
    _ForceStrength ("External Force Strength", Range(0, 10)) = 5
    _BendGain ("Bend Gain", Range(0, 10)) = 0.6
    _CurvePow ("Curve Power", Range(0, 10)) = 2
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
      #pragma multi_compile _ _LIGHT_LAYERS
      #pragma multi_compile _ _FORWARD_PLUS
      #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
      #pragma multi_compile _ SHADOWS_SHADOWMASK
      #pragma multi_compile _ DIRLIGHTMAP_COMBINED
      #pragma multi_compile _ LIGHTMAP_ON
      #pragma multi_compile _ DYNAMICLIGHTMAP_ON

      #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
      #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
      #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
      #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
      #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
      #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
      #pragma multi_compile_fragment _ _LIGHT_COOKIES

      #pragma vertex vert
      #pragma fragment frag

      // Very important for specular
      #define _SPECULAR_COLOR
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
      #include "Assets/Shaders/Includes/Bezier.hlsl"
      #include "Assets/Shaders/Includes/Noise.hlsl"

      // #define DEBUG_SHOW_NORMALS

      // Needed for proper indirect drawing (though I cannot see why so far)
      #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
      #include "UnityIndirect.cginc"

      static float DEG_TO_RAD = 2 * 3.14159 / 360;

      struct GrassInstance
      {
        float3 position;
      };

      void Scale(inout float3 posOS, float radius, float height) {
        posOS.x *= radius;
        posOS.y *= height;
      }

      void RotateAboutOrigin(inout float3 posOS, float maxRotationRadians, float maxHeight)
      {
        float h = saturate(posOS.y / maxHeight);
        float w = h;
        float angle = maxRotationRadians * w;
        float s, c;
        sincos(angle, s, c);
        float y = posOS.y;
        float z = posOS.z;
        posOS.y = y * c - z * s;
        posOS.z = y * s + z * c;
      }

      void RotateNormalAboutOrigin(inout float3 normalOS, float3 posOS, float maxRotationRadians, float maxHeight)
      {
        float h = saturate(posOS.y / maxHeight);
        float w = h;
        float angle = maxRotationRadians * w * 2.;
        float s, c;
        sincos(angle, s, c);
        float y = normalOS.y;
        float z = normalOS.z;
        normalOS.y = y * c - z * s;
        normalOS.z = y * s + z * c;
        normalOS = normalize(normalOS);
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

      float3x3 RotAxis(float3 u, float a) {
        u = normalize(u);
        float s, c;
        sincos(a, s, c);
        float x = u.x;
        float y = u.y;
        float z = u.z;
        float ic = 1.0 - c;
        return float3x3(
        c + ic * x * x, ic * x * y + z * s, ic * x * z - y * s,
        ic * y * x - z * s, c + ic * y * y, ic * y * z + x * s,
        ic * z * x + y * s, ic * z * y - x * s, c + ic * z * z
        );
      }

      void ApplyExternalForce(
      inout float3 pLS,
      inout float3 nLS,
      float bladeHeight,
      float3 forceDirLS,
      float forceStrength,
      float bendGain,
      float curvePow
      ) {
        const float3 up = float3(0, 1, 0);

        if (bladeHeight <= 0.0 || forceStrength <= 0.0) {
          return;
        }
        float u = saturate(pLS.y / bladeHeight);
        float2 fxz = forceDirLS.xz;
        float fxzLen = length(fxz);

        if (fxzLen < .00001) {
          return;
        }

        float3 f = float3(
          fxz.x / fxzLen,
          0.0,
          fxz.y / fxzLen);
        float3 axis = cross(up, f);
        float axisLen2 = dot(axis, axis);
        if (axisLen2 < .000000001) {
          return;
        }

        float angle = bendGain * forceStrength * pow(u, curvePow);
        float3x3 R = RotAxis(normalize(axis), angle);
        pLS = mul(R, pLS);
        nLS = normalize(mul(R, nLS));
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

      float _ForceStrength;
      float _BendGain;
      float _CurvePow;

      StructuredBuffer<GrassInstance> GrassInstances;

      Varyings vert (Attributes input, uint instanceID : SV_INSTANCEID)
      {
        Varyings varyings = (Varyings)0;
        InitIndirectDrawArgs(0);
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, varyings);

        float3 _ForceDirection = float3(1, 0, 0);
        float3 instancePos = GrassInstances[GetIndirectInstanceID(instanceID)].position;
        float3 positionOS = input.positionOS.xyz;
        float3 normalOS = input.normalOS;
        float bladeHeight = .5f * _BladeHeight + .5 * _BladeHeight * abs(noise2D(instancePos.xz));
        RotateAboutOrigin(positionOS, _BladeCurve, bladeHeight);
        Scale(positionOS, _BladeRadius, bladeHeight);
        BendNormalAboutY(normalOS, clamp(input.positionOS.x / _BladeRadius, - 1, 1) * _NormalBend);
        RotateNormalAboutOrigin(normalOS, input.positionOS.xyz, _BladeCurve, bladeHeight);
        GrassNoiseRotateY(positionOS, normalOS, instancePos.xz, _LowFrequency, DEG_TO_RAD * _MaxYRotationLowFrequency);
        GrassNoiseRotateY(positionOS, normalOS, instancePos.xz, _HighFrequency, DEG_TO_RAD * _MaxYRotationHighFrequency);
        ApplyExternalForce(
        positionOS,
        normalOS,
        bladeHeight,
        _ForceDirection,
        _ForceStrength * abs(noise2D(3 * instancePos.xz + _Time.y)),
        _BendGain,
        _CurvePow);

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
        inputData.normalWS = (isFrontFace ? 1 : - 1) * normalize(varyings.normalWS);
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