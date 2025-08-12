float2 hash(float2 p)
{
  p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
  return - 1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

float2 hash2(uint seed)
{
  float2 p = float2(seed, seed * 1664525u + 1013904223u);
  return frac(sin(p) * float2(43758.5453, 22578.145912));
}

float hash21(float2 p)
{
  p = frac(p * float2(123.34, 456.21));
  p += dot(p, p + 78.233);
  return frac(p.x * p.y);
}

uint Hash32(uint x)
{
  x ^= 2747636419u;
  x *= 2654435769u;
  x ^= x >> 16;
  x *= 2654435769u;
  x ^= x >> 16;
  x *= 2654435769u;
  return x;
}

float Hash01(uint seed)
{
  uint h = Hash32(seed);
  return (h >> 8) * (1.0 / 16777216.0);
}

uint Mix2(uint a, uint b)
{
  return Hash32(a ^ (b * 2246822519u + 3266489917u));
}

uint Mix3(uint a, uint b, uint c)
{
  return Hash32(a ^ (b * 2246822519u) ^ (c * 3266489917u));
}

float noise(float2 p)
{
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

float noise2D(float2 p)
{
  float2 i = floor(p);
  float2 f = frac(p);
  float a = hash21(i);
  float b = hash21(i + float2(1, 0));
  float c = hash21(i + float2(0, 1));
  float d = hash21(i + float2(1, 1));
  float2 u = f * f * (3.0 - 2.0 * f);
  return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y) * 2.0 - 1.0;
}

float valueNoise(float2 p)
{
  float2 i = floor(p);
  float2 f = frac(p);
  float a = frac(sin(dot(i + float2(0, 0), float2(12.9898, 78.233))) * 43758.5453);
  float b = frac(sin(dot(i + float2(1, 0), float2(12.9898, 78.233))) * 43758.5453);
  float c = frac(sin(dot(i + float2(0, 1), float2(12.9898, 78.233))) * 43758.5453);
  float d = frac(sin(dot(i + float2(1, 1), float2(12.9898, 78.233))) * 43758.5453);
  float2 u = f * f * (3.0 - 2.0 * f);
  return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}
