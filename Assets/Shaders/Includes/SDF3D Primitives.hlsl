float SDF_Sphere(float3 p, float3 center, float radius) {
  return length(p - center) - radius;
}