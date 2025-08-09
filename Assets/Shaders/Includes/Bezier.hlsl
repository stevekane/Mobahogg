void BezierEval(float3 b, float3 c, float3 d, float t, out float3 p, out float3 n) {
  float s = 1.0 - t;
  float s2 = s * s, t2 = t * t;
  float k1 = 3.0 * s2 * t;
  float k2 = 3.0 * s * t2;
  float k3 = t * t2;
  p = k1 * b + k2 * c + k3 * d;

  float3 dp = (k1 - 2.0 * k2) * b + (2.0 * k2 - 3.0 * k3) * c + 3.0 * t2 * d;
  float3 tHat = normalize(dp);

  float3 upA = float3(0,1,0), upB = float3(1,0,0);
  float3 up = lerp(upB, upA, step(abs(dot(tHat, upA)), 0.99));
  float3 bHat = normalize(cross(up, tHat));
  n = cross(tHat, bHat);
}