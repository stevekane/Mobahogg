Shader "Custom/SDF Renderer"
{
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

      struct SphereData {
        float3 center;
        float radius;
        float3 stretchAxis;
        float stretchFraction;
        float inverseSquareRootStretchFraction; // computed on CPU to avoid cost on GPU per-pixel
      };

      StructuredBuffer<SphereData> _Spheres;
      uniform int _SphereCount;
      uniform float4 _Color;
      uniform float _EmissionIntensity;
      uniform float _FresnelPower;
      uniform float _EdgeThreshold;

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

      float3 EstimateNormal(float3 p) {
        const float eps = 0.001;
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

      float3 ComputeRayDirection(float2 uv) {
        float4 clipPos = float4(uv * 2.0 - 1.0, 0.0, 1.0);
        float4 worldNear = mul(UNITY_MATRIX_I_VP, clipPos);
        worldNear.xyz /= worldNear.w;

        clipPos.z = 1.0;
        float4 worldFar = mul(UNITY_MATRIX_I_VP, clipPos);
        worldFar.xyz /= worldFar.w;

        return normalize(worldFar.xyz - worldNear.xyz);
      }

      float EdgeDetectNormal(
        float2 uv,
        float3 normal,
        float2 texelSize,
        float3 camPos,
        float4x4 invVP,
        float threshold
      ) {
        float2 dx = float2(texelSize.x, 0.0);
        float2 dy = float2(0.0, texelSize.y);

        float3 dummyN;
        float dummyD;
        float dummyVal;
        float3 nX = 0, nY = 0;

        RenderSDF_float(uv + dx, camPos, invVP, dummyVal, nX, dummyD);
        RenderSDF_float(uv + dy, camPos, invVP, dummyVal, nY, dummyD);
        float diffX = 1.0 - dot(normal, normalize(nX));
        float diffY = 1.0 - dot(normal, normalize(nY));
        float edge = saturate((diffX + diffY) * 0.5 / threshold);
        return edge;
      }

      float EdgeDetectHybrid(
        float2 uv,
        float3 normal,
        float rawDepth,
        float2 texelSize,
        float3 camPos,
        float4x4 invVP,
        float normalThreshold,
        float depthThreshold
      ) {
        float2 dx = float2(texelSize.x, 0);
        float2 dy = float2(0, texelSize.y);
        float3 nX, nY;
        float dX, dY;
        float v;
        RenderSDF_float(uv + dx, camPos, invVP, v, nX, dX);
        RenderSDF_float(uv + dy, camPos, invVP, v, nY, dY);
        float normalDiffX = 1.0 - dot(normal, normalize(nX));
        float normalDiffY = 1.0 - dot(normal, normalize(nY));
        float normalEdge = max(normalDiffX, normalDiffY);
        float depthDiffX = abs(rawDepth - dX);
        float depthDiffY = abs(rawDepth - dY);
        float depthEdge = max(depthDiffX, depthDiffY);
        depthEdge = depthEdge / (rawDepth + 1e-3); // prevent divide-by-zero
        float nTerm = saturate(normalEdge / normalThreshold);
        float dTerm = saturate(depthEdge / depthThreshold);
        float suppress = smoothstep(0.0, 0.5, dot(normal, normalize(camPos - (camPos + normalize(normal) * rawDepth))));
        return saturate((nTerm + dTerm) * (1.0 - suppress));
      }

      float EdgeDetectContourOnly(
        float2 uv,
        float rawDepth,
        float2 texelSize,
        float3 camPos,
        float4x4 invVP,
        float threshold
      ) {
        float2 dx = float2(texelSize.x, 0);
        float2 dy = float2(0, texelSize.y);
        float3 dummyN;
        float dX, dY;
        float v;
        RenderSDF_float(uv + dx, camPos, invVP, v, dummyN, dX);
        RenderSDF_float(uv + dy, camPos, invVP, v, dummyN, dY);
        float diffX = abs(rawDepth - dX);
        float diffY = abs(rawDepth - dY);
        float edge = max(diffX, diffY);
        edge /= max(rawDepth, 1e-3);
        return saturate(edge / threshold);
      }


      float CalculateFresnel(
        float3 viewDir,
        float3 normal,
        float fresnelPower
      ) {
        float f = dot(normal, viewDir);
        f = 1.0 - abs(f);
        f = pow(f, fresnelPower);
        return f;
      }

      Varyings Vert (Attributes input) {
        Varyings output;
        output.positionCS = input.positionOS;
        output.uv = input.uv;
        return output;
      }

      struct FragmentOutput {
        float4 color : SV_Target;
        float depth : SV_Depth;
      };

      FragmentOutput Frag (Varyings input) {
        const float4 JET_BLACK = float4(0, 0, 0, 1);
        float4 EmissionColor = float4(_EmissionIntensity, _EmissionIntensity, _EmissionIntensity, 1);
        float3 camPos = _WorldSpaceCameraPos;
        float4x4 invVP = UNITY_MATRIX_I_VP;
        float value, rawDepth;
        float3 normal;
        FragmentOutput o;

        RenderSDF_float(input.uv, camPos, invVP, value, normal, rawDepth);

        if (value < 0.5) discard;

        float3 rayDir = ComputeRayDirection(input.uv);
        float3 worldPos = camPos + rayDir * rawDepth;
        float3 viewDir = normalize(camPos - worldPos);
        // ScreenParams.z and w are 1+1/width and 1+1/height respectively
        float texelSize = _ScreenParams.zw - 1;
        float threshold = _EdgeThreshold;
        float corona = EdgeDetectContourOnly(
          input.uv,
          rawDepth,
          texelSize,
          camPos,
          invVP,
          threshold);

        o.color = lerp(JET_BLACK, EmissionColor, corona);
        o.depth = rawDepth;
        return o;
      }

      ENDHLSL
    }
  }
  FallBack "Hidden/InternalErrorShader"
}
