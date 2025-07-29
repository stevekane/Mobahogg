void FullScreenQuadFromVertexIDs(uint vertexID, out float2 uv, out float4 positionCS) {
  uv.x = vertexID == 2 ? 2.0 : 0.0;
  uv.y = vertexID == 1 ? 2.0 : 0.0;
  positionCS.x = uv.x * 2.0 - 1.0,
  positionCS.y = uv.y * 2.0 - 1.0,
  positionCS.z = 0.0;
  positionCS.w = 1.0;
}