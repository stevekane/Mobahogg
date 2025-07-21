Shader "SDF_Renderer/DepthMask"
{
  FallBack "Hidden/InternalErrorShader"
  SubShader {
    Pass {
      Name "SDF Depth"

      Cull Off
      Blend Off
      ZTest Off
      ZWrite On

      HLSLPROGRAM
      #pragma vertex Vert
      #pragma fragment Frag
      #pragma target 4.5

      // TODO: Do we actually need these? I kind of doubt it
      // URP Lighting Keywords
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
      #pragma multi_compile _ _SHADOWS_SOFT

      #define ATTRIBUTES_NEED_TEXCOORD0
      #define ATTRIBUTES_NEED_TEXCOORD1
      #define ATTRIBUTES_NEED_VERTEXID
      #define VARYINGS_NEED_TEXCOORD0
      #define VARYINGS_NEED_TEXCOORD1
      #define REQUIRE_DEPTH_TEXTURE

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Full Screen Utils.cginc"

      struct Attributes {
        uint vertexID : SV_VertexID;
      };

      struct Varyings {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      struct SphereData {
        float3 center;
        float radius;
        float3 stretchAxis;
        float stretchFraction;
        float inverseSquareRootStretchFraction; // computed on CPU to avoid cost on GPU per-pixel
      };

      StructuredBuffer<SphereData> _Spheres;
      int _SphereCount;

      float sdSphereStretchVolumePreserved(
        float3 p,
        float3 center,
        float radius,
        float3 axis,
        float stretchAmt,
        float invRootStretchAmt
      ) {
        float3 scale = float3(1.0, 1.0, 1.0);
        float3 stretch = axis * stretchAmt;
        float3 perp = 1.0 - axis;
        scale = axis * stretchAmt + perp * invRootStretchAmt;
        float3 q = (p - center) / scale;
        float dist = length(q) - radius;
        float correction = min(scale.x, min(scale.y, scale.z));
        return dist * correction;
      }

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
          SphereData sphere = _Spheres[i];
          float dist = sdSphereStretchVolumePreserved(
            p,
            sphere.center,
            sphere.radius,
            sphere.stretchAxis,
            sphere.stretchFraction,
            sphere.inverseSquareRootStretchFraction);
          d = SmoothMin(d, dist, 0.5);
        }
        return d;
      }

      bool Raymarch(
      float3 ro,
      float3 rd,
      out float3 hitPos,
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
            distOut = t;
            return true;
          }
          if (t > maxDist) break;
          t += d;
        }

        hitPos = float3(0,0,0);
        distOut = maxDist;
        return false;
      }

      void RenderSDF_float(
        float2 uv,
        float3 camPos,
        float4x4 invViewProj,
        out float value,
        out float rawDepth
      ) {
        float4 ndc = float4(uv * 2.0 - 1.0, 1.0, 1.0); // Far plane
        float4 worldFar = mul(invViewProj, ndc);
        worldFar.xyz /= worldFar.w;
        float3 rayDir = normalize(worldFar.xyz - camPos);
        float3 hitPos;
        float dist;

        if (Raymarch(camPos, rayDir, hitPos, dist)) {
          value = 1.0;
          float4 clipPos = mul(UNITY_MATRIX_VP, float4(hitPos, 1.0));
          rawDepth = clipPos.z / clipPos.w;
        } else {
          value = 0.0;
          rawDepth = 1.0; // far plane
        }
      }

      float3 ComputeRayDirection(float2 uv) {
        float4 clipPos = float4(uv * 2.0 - 1.0, 0.0, 1.0);
        float4 worldNear = mul(UNITY_MATRIX_I_VP, clipPos);
        worldNear.xyz /= worldNear.w;
        clipPos.z = 1.0;
        float4 worldFar = mul(UNITY_MATRIX_I_VP, clipPos);
        worldFar.xyz /= worldFar.w;
        return normalize(worldFar.xyz - worldNear.xyz);
      }

      Varyings Vert (Attributes input) {
        Varyings output;
        FullScreenQuadFromVertexIDs(input.vertexID, output.uv, output.positionCS);
        return output;
      }

      struct FragmentOutput {
        float4 normal : SV_TARGET;
        float depth : SV_DEPTH;
      };

      FragmentOutput Frag (Varyings input) {
        float3 camPos = _WorldSpaceCameraPos;
        float4x4 invVP = UNITY_MATRIX_I_VP;
        float value, rawDepth;
        FragmentOutput o;
        RenderSDF_float(input.uv, camPos, invVP, value, rawDepth);
        // TODO: What is value??

        if (value < 0.5) discard;
        o.depth = rawDepth;
        o.normal = float4(1, 1, 1, 1);
        return o;
      }

      ENDHLSL
    }
  }
}