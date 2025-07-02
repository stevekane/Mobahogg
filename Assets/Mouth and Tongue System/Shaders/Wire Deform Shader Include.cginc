float WireSectionSmoothMask(
float min,
float max,
float value,
float dampedOscillationFraction
) {
  float distance = max - min;
  float edge = distance * dampedOscillationFraction;
  float centerStart = min + edge;
  float centerEnd = max - edge;
  float mask = smoothstep(min, centerStart, value) * smoothstep(max, centerEnd, value);
  return mask;
}

void ApplyWireOscillation_float(
float3 positionWS,
float3 _WireStart,
float3 _WireEnd,
float _WaveLength,
float _WaveAmplitude,
float _Offset,
float _DampedOscillationFraction,
out float3 Out) {
  float3 start = _WireStart.xyz;
  float3 end = _WireEnd.xyz;
  float3 wireVec = end - start;
  float wireLength = length(wireVec);
  float3 wireDir = normalize(wireVec);
  float3 toPoint = positionWS - start;
  float x = dot(toPoint, wireDir);
  float mask = WireSectionSmoothMask(0, wireLength, x, _DampedOscillationFraction);
  float3 projected = start + wireDir * x;
  float3 radial = cross(float3(0,1,0), wireDir);
  float wavePhase = 6.28318 * (x / _WaveLength) + _Offset;
  float offset = _WaveAmplitude * sin(wavePhase);
  Out = positionWS + mask * offset * radial;
}