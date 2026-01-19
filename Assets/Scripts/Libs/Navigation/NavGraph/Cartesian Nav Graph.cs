using System;
using UnityEngine;

[Serializable]
public sealed class CartesianNavGraph : INavGrid
{
  [SerializeField] float _cellSize;
  [SerializeField] Vector3 _origin;
  [SerializeField] float _halfWidth;
  [SerializeField] float _halfHeight;

  [SerializeField] int _width;
  [SerializeField] int _height;
  [SerializeField] NavCellTag[] _tags;

  public float CellSize => _cellSize;
  public Vector3 Origin => _origin;
  public float HalfWidth => _halfWidth;
  public float HalfHeight => _halfHeight;

  public int Width => _width;
  public int Height => _height;
  public int CellCount => _width * _height;

  public CartesianNavGraph(float cellSize, Vector3 origin, float halfWidth, float halfHeight)
  {
    _cellSize = Mathf.Max(0.0001f, cellSize);
    _origin = origin;
    _halfWidth = Mathf.Max(0f, halfWidth);
    _halfHeight = Mathf.Max(0f, halfHeight);

    _width = Mathf.Max(1, Mathf.CeilToInt((2f * _halfWidth) / _cellSize));
    _height = Mathf.Max(1, Mathf.CeilToInt((2f * _halfHeight) / _cellSize));
    _tags = new NavCellTag[_width * _height];
  }

  public ref NavCellTag TagRef(int x, int z) => ref _tags[z * _width + x];

  public bool TryGetTag(Vector3 worldXZ, out NavCellTag tag)
  {
    if (!TryWorldToCell(worldXZ, out int x, out int z))
    {
      tag = NavCellTag.Void;
      return false;
    }
    tag = _tags[z * _width + x];
    return true;
  }

  public bool TryGetTag(int a, int b, out NavCellTag tag)
  {
    int x = a;
    int z = b;
    if ((uint)x >= (uint)_width || (uint)z >= (uint)_height)
    {
      tag = NavCellTag.Void;
      return false;
    }
    tag = _tags[z * _width + x];
    return true;
  }

  public bool TryWorldToCell(Vector3 world, out int x, out int z)
  {
    Vector3 p = world - _origin;

    float fx = (p.x + _halfWidth) / _cellSize;
    float fz = (p.z + _halfHeight) / _cellSize;

    x = (int)Mathf.Floor(fx);
    z = (int)Mathf.Floor(fz);

    return (uint)x < (uint)_width && (uint)z < (uint)_height;
  }

  public Vector3 CellCenterWorld(int x, int z)
  {
    float wx = ((x + 0.5f) * _cellSize) - _halfWidth;
    float wz = ((z + 0.5f) * _cellSize) - _halfHeight;
    return _origin + new Vector3(wx, 0f, wz);
  }

  public void ForEachCell(Func<int, int, Vector3, NavCellTag, bool> visitor)
  {
    for (int z = 0; z < _height; z++)
      for (int x = 0; x < _width; x++)
      {
        var tag = _tags[z * _width + x];
        var c = CellCenterWorld(x, z);
        if (!visitor(x, z, c, tag)) return;
      }
  }

  // Visits every cell intersected by the XZ ray segment.
  // Return false from visitor to stop early.
  public void RayWalk(Vector3 worldFrom, Vector3 worldDir, float maxDistance, Func<int, int, bool> visitor)
  {
    Vector3 dir = worldDir;
    dir.y = 0f;
    float len = dir.magnitude;
    if (len <= 1e-6f) return;
    dir /= len;

    float dist = Mathf.Max(0f, maxDistance);

    Vector3 p0 = worldFrom - _origin;
    float gx = (p0.x + _halfWidth) / _cellSize;
    float gz = (p0.z + _halfHeight) / _cellSize;

    int x = (int)Mathf.Floor(gx);
    int z = (int)Mathf.Floor(gz);

    if ((uint)x >= (uint)_width || (uint)z >= (uint)_height)
      return;

    float dx = dir.x;
    float dz = dir.z;

    int stepX = dx >= 0f ? 1 : -1;
    int stepZ = dz >= 0f ? 1 : -1;

    float nextBoundaryX = (dx >= 0f) ? (x + 1) : x;
    float nextBoundaryZ = (dz >= 0f) ? (z + 1) : z;

    float tMaxX = (Mathf.Abs(dx) < 1e-6f) ? float.PositiveInfinity : (nextBoundaryX - gx) / dx;
    float tMaxZ = (Mathf.Abs(dz) < 1e-6f) ? float.PositiveInfinity : (nextBoundaryZ - gz) / dz;

    float tDeltaX = (Mathf.Abs(dx) < 1e-6f) ? float.PositiveInfinity : (1f / Mathf.Abs(dx));
    float tDeltaZ = (Mathf.Abs(dz) < 1e-6f) ? float.PositiveInfinity : (1f / Mathf.Abs(dz));

    float t = 0f;
    float maxT = dist / _cellSize;

    if (!visitor(x, z)) return;

    while (t <= maxT)
    {
      if (tMaxX < tMaxZ)
      {
        t = tMaxX;
        tMaxX += tDeltaX;
        x += stepX;
        if ((uint)x >= (uint)_width) break;
      }
      else
      {
        t = tMaxZ;
        tMaxZ += tDeltaZ;
        z += stepZ;
        if ((uint)z >= (uint)_height) break;
      }

      if (!visitor(x, z)) return;
    }
  }

  public void AppendAdjacentCells(
  int x,
  int z,
  AppendOnly<GridNeighbor> outNeighbors)
  {
    const float SQRT2 = 1.41421356f;
    outNeighbors.Append(new GridNeighbor(x + 1, z, 1f));
    outNeighbors.Append(new GridNeighbor(x - 1, z, 1f));
    outNeighbors.Append(new GridNeighbor(x, z + 1, 1f));
    outNeighbors.Append(new GridNeighbor(x, z - 1, 1f));
    outNeighbors.Append(new GridNeighbor(x + 1, z + 1, SQRT2));
    outNeighbors.Append(new GridNeighbor(x + 1, z - 1, SQRT2));
    outNeighbors.Append(new GridNeighbor(x - 1, z + 1, SQRT2));
    outNeighbors.Append(new GridNeighbor(x - 1, z - 1, SQRT2));
  }
}