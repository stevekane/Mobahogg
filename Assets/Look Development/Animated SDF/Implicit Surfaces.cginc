float SDF_Sphere(float3 p, float3 center, float radius) {
  return length(p - center) - radius;
}

float SmoothMin(float d1, float d2, float k) {
  float h = saturate(0.5 + 0.5 * (d2 - d1) / k);
  return lerp(d2, d1, h) - k * h * (1.0 - h);
}

float SceneSDF(float3 p) {
  float d1 = SDF_Sphere(p, float3(-1.5, .5, 0), 1);
  float d2 = SDF_Sphere(p, float3(1.0, 0, 0), 1);
  float d3 = SDF_Sphere(p, float3(0.0, 2.5, 0), 1.5);
  return SmoothMin(SmoothMin(d1, d2, 1), d3, 1);
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

bool Raymarch(float3 ro, float3 rd, out float3 hitPos, out float3 normal, out float distOut) {
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