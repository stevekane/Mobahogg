float SmoothMin(float d1, float d2, float k) {
  float h = saturate(0.5 + 0.5 * (d2 - d1) / k);
  return lerp(d2, d1, h) - k * h * (1.0 - h);
}