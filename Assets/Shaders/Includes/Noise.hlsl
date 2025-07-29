float2 hash(float2 p) {
  p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
  return - 1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

float noise(float2 p) {
  float2 i = floor(p);
  float2 f = frac(p);

  float2 u = f * f * (3.0 - 2.0 * f); // Smoothstep curve

  float2 a = hash(i + float2(0.0, 0.0));
  float2 b = hash(i + float2(1.0, 0.0));
  float2 c = hash(i + float2(0.0, 1.0));
  float2 d = hash(i + float2(1.0, 1.0));

  float res = lerp(lerp(dot(a, f - float2(0.0, 0.0)), dot(b, f - float2(1.0, 0.0)), u.x),
  lerp(dot(c, f - float2(0.0, 1.0)), dot(d, f - float2(1.0, 1.0)), u.x), u.y);
  return res;
}