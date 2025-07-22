Shader "SDF_Renderer/DepthMask"
{
  Properties {
    _UnionDistance ("Union Distance", Range(0.0, 10.0)) = 0.5
  }
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

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Full Screen Utils.cginc"

      struct Attributes {
        uint vertexID : SV_VertexID;
      };

      struct Varyings {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      struct FragmentOutput {
        float mask : SV_TARGET;
        float depth : SV_DEPTH;
      };

      struct SphereData {
        float3 center;
        float radius;
        float3 stretchAxis;
        float stretchFraction;
        float inverseSquareRootStretchFraction; // computed on CPU to avoid cost on GPU per - pixel
      };

      StructuredBuffer<SphereData> _Spheres;
      int _SphereCount;
      float _UnionDistance;

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

      float SDF_Scene(float3 p) {
        float d = 10000.0; // start with large distance
        for (int i = 0; i < _SphereCount; i ++) {
          SphereData sphere = _Spheres[i];
          float dist = sdSphereStretchVolumePreserved(
          p,
          sphere.center,
          sphere.radius,
          sphere.stretchAxis,
          sphere.stretchFraction,
          sphere.inverseSquareRootStretchFraction);
          d = SmoothMin(d, dist, _UnionDistance);
        }
        return d;
      }

      float3 RayDirection(float2 uv) {
        float4 ndc = float4(uv * 2.0 - 1.0, 1.0, 1.0); // Far plane
        float4 worldFar = mul(UNITY_MATRIX_I_VP, ndc);
        worldFar.xyz /= worldFar.w;
        float3 rayDir = normalize(worldFar.xyz - _WorldSpaceCameraPos);
        return rayDir;
      }

      bool RayMarch(
      float3 ro,
      float3 rd,
      out float3 hitPos,
      out float distOut) {
        const int maxSteps = 256;
        const float maxDist = 10000.0;
        const float surfEps = 0.001;
        float t = 0.0;

        for (int i = 0; i < maxSteps; i ++) {
          float3 p = ro + t * rd;
          float d = SDF_Scene(p);
          if (d < surfEps) {
            hitPos = p;
            distOut = t;
            return true;
          }
          if (t > maxDist) break;
          t += d;
        }

        hitPos = float3(0, 0, 0);
        distOut = maxDist;
        return false;
      }

      Varyings Vert (Attributes input) {
        Varyings output;
        FullScreenQuadFromVertexIDs(input.vertexID, output.uv, output.positionCS);
        return output;
      }

      FragmentOutput Frag (Varyings input) {
        FragmentOutput o;
        float value, hitDistance;
        float3 hitPosition;
        float3 rayOrigin = _WorldSpaceCameraPos;
        float3 rayDirection = RayDirection(input.uv);

        if (! RayMarch(rayOrigin, rayDirection, hitPosition, hitDistance)) {
          discard;
        }
        float4 clipPos = mul(UNITY_MATRIX_VP, float4(hitPosition, 1.0));
        o.depth = clipPos.z / clipPos.w;
        o.mask = 1;
        return o;
      }

      ENDHLSL
    }
  }
}