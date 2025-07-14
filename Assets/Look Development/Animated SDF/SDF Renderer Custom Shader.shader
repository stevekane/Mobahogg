Shader "Custom/SDFRaymarchURPWithDepth"
{
  Properties {
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

      struct Attributes {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct Varyings {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      #define MAX_SPHERES 16
      uniform int _SphereCount;
      uniform float4 _Spheres[MAX_SPHERES]; // xyz = center, w = radius

      float SDF_Sphere(float3 p, float3 center, float radius) {
        return length(p - center) - radius;
      }

      float SmoothMin(float d1, float d2, float k) {
        float h = saturate(0.5 + 0.5 * (d2 - d1) / k);
        return lerp(d2, d1, h) - k * h * (1.0 - h);
      }

      float SceneSDF(float3 p) {
        float d = 10000.0; // start with large distance
        for (int i = 0; i < _SphereCount; i++) {
          float4 s = _Spheres[i];
          float sphereDist = length(p - s.xyz) - s.w;
          d = SmoothMin(d, sphereDist, 0.5);
        }
        return d;
      }

      float3 EstimateNormal(float3 p) {
        float eps = 0.001;
        float3 dx = float3(eps, 0, 0);
        float3 dy = float3(0, eps, 0);
        float3 dz = float3(0, 0, eps);

        return normalize(float3(
          SceneSDF(p + dx) - SceneSDF(p - dx),
          SceneSDF(p + dy) - SceneSDF(p - dy),
          SceneSDF(p + dz) - SceneSDF(p - dz)
        ));
      }

      bool Raymarch(
      float3 ro,
      float3 rd,
      out float3 hitPos,
      out float3 normal,
      out float distOut) {
        const int maxSteps = 128;
        const float maxDist = 100.0;
        const float surfEps = 0.001;
        float t = 0.0;

        for (int i = 0; i < maxSteps; i++) {
          float3 p = ro + t * rd;
          float d = SceneSDF(p);
          if (d < surfEps) {
            hitPos = p;
            normal = EstimateNormal(p);
            distOut = t;
            return true;
          }
          if (t > maxDist) break;
          t += d;
        }

        hitPos = float3(0,0,0);
        normal = float3(0,1,0);
        distOut = maxDist;
        return false;
      }

      void RenderSDF_float(
        float2 uv,
        float3 camPos,
        float4x4 invViewProj,
        out float value,
        out float3 normal,
        out float rawDepth
      ) {
        // Compute world-space ray
        float4 ndc = float4(uv * 2.0 - 1.0, 1.0, 1.0); // Far plane
        float4 worldFar = mul(invViewProj, ndc);
        worldFar.xyz /= worldFar.w;

        float3 rayDir = normalize(worldFar.xyz - camPos);
        float3 hitPos;
        float dist;

        if (Raymarch(camPos, rayDir, hitPos, normal, dist)) {
          value = 1.0;
          float4 clipPos = mul(UNITY_MATRIX_VP, float4(hitPos, 1.0));
          rawDepth = clipPos.z / clipPos.w;
        } else {
          value = 0.0;
          normal = float3(0, 0, 1);
          rawDepth = 1.0; // far plane
        }
  }

      Varyings Vert (Attributes input) {
        Varyings output;
        // This is the normal behavior to cast objectspace to clipspace
        // However, since our geo here is a fullscreen quad it's already
        // in clipspace so don't do this
        // output.positionCS = TransformObjectToHClip(input.positionOS);
        output.positionCS = input.positionOS;
        output.uv = input.uv;
        return output;
      }

      struct FragmentOutput {
        float4 color : SV_Target;
        float depth : SV_Depth;
      };

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
