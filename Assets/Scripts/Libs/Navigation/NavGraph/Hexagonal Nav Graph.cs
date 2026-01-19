using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class HexagonalNavGraph : INavGrid
{
  [SerializeField] float _cellSize;
  [SerializeField] Vector3 _origin;
  [SerializeField] float _halfWidth;
  [SerializeField] float _halfHeight;

  [SerializeField] NavCellTag[] _tags;
  [SerializeField] Vector3[] _centers;
  [SerializeField] int[] _a; // q
  [SerializeField] int[] _b; // r

  [NonSerialized] Dictionary<long, int> _index;

  public float CellSize => _cellSize;
  public Vector3 Origin => _origin;
  public float HalfWidth => _halfWidth;
  public float HalfHeight => _halfHeight;

  public int CellCount => _tags?.Length ?? 0;

  public HexagonalNavGraph(float cellSize, Vector3 origin, float halfWidth, float halfHeight)
  {
    _cellSize = Mathf.Max(0.0001f, cellSize);
    _origin = origin;
    _halfWidth = Mathf.Max(0f, halfWidth);
    _halfHeight = Mathf.Max(0f, halfHeight);

    BuildLayout();
    RebuildIndexIfNeeded();
  }

  public bool TryWorldToCell(Vector3 world, out int q, out int r)
  {
    return TryWorldToAxial(world, out q, out r);
  }

  public Vector3 CellCenterWorld(int q, int r)
  {
    return AxialCenterWorld(q, r);
  }

  public bool TryGetTag(int q, int r, out NavCellTag tag)
  {
    RebuildIndexIfNeeded();

    if (_index.TryGetValue(Pack(q, r), out int idx))
    {
      tag = _tags[idx];
      return true;
    }

    tag = NavCellTag.Void;
    return false;
  }

  public void SetTagAtIndex(int idx, NavCellTag tag) => _tags[idx] = tag;
  public Vector3 GetCenterAtIndex(int idx) => _centers[idx];
  public void GetCellAtIndex(int idx, out int q, out int r) { q = _a[idx]; r = _b[idx]; }

  public void ForEachCell(Func<int, int, Vector3, NavCellTag, bool> visitor)
  {
    for (int i = 0; i < _centers.Length; i++)
    {
      if (!visitor(_a[i], _b[i], _centers[i], _tags[i])) return;
    }
  }

  public bool TryWorldToAxial(Vector3 world, out int q, out int r)
  {
    Vector3 p = world - _origin;
    float s = _cellSize;

    float qf = (1.7320508075688772f / 3f * p.x - 1f / 3f * p.z) / s;
    float rf = (2f / 3f * p.z) / s;

    float xf = qf;
    float zf = rf;
    float yf = -xf - zf;

    int xi = Mathf.RoundToInt(xf);
    int yi = Mathf.RoundToInt(yf);
    int zi = Mathf.RoundToInt(zf);

    float dx = Mathf.Abs(xi - xf);
    float dy = Mathf.Abs(yi - yf);
    float dz = Mathf.Abs(zi - zf);

    if (dx > dy && dx > dz) xi = -yi - zi;
    else if (dy > dz) yi = -xi - zi;
    else zi = -xi - yi;

    q = xi;
    r = zi;

    return true;
  }

  public Vector3 AxialCenterWorld(int q, int r)
  {
    float s = _cellSize;
    float x = s * 1.7320508075688772f * (q + r * 0.5f);
    float z = s * 1.5f * r;
    return _origin + new Vector3(x, 0f, z);
  }

  public void RayWalk(Vector3 worldFrom, Vector3 worldDir, float maxDistance, Func<int, int, bool> visitor)
  {
    Vector3 dir = worldDir;
    dir.y = 0f;
    float len = dir.magnitude;
    if (len <= 1e-6f) return;
    dir /= len;

    float dist = Mathf.Max(0f, maxDistance);
    Vector3 worldTo = worldFrom + dir * dist;

    TryWorldToAxial(worldFrom, out int q0, out int r0);
    TryWorldToAxial(worldTo, out int q1, out int r1);

    int x0 = q0;
    int z0 = r0;
    int y0 = -x0 - z0;

    int x1 = q1;
    int z1 = r1;
    int y1 = -x1 - z1;

    int n = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0), Mathf.Abs(z1 - z0));
    if (n == 0)
    {
      visitor(q0, r0);
      return;
    }

    long lastKey = long.MinValue;

    for (int i = 0; i <= n; i++)
    {
      float t = (float)i / n;
      float xf = Mathf.Lerp(x0, x1, t);
      float yf = Mathf.Lerp(y0, y1, t);
      float zf = Mathf.Lerp(z0, z1, t);

      int xi = Mathf.RoundToInt(xf);
      int yi = Mathf.RoundToInt(yf);
      int zi = Mathf.RoundToInt(zf);

      float dx = Mathf.Abs(xi - xf);
      float dy = Mathf.Abs(yi - yf);
      float dz = Mathf.Abs(zi - zf);

      if (dx > dy && dx > dz) xi = -yi - zi;
      else if (dy > dz) yi = -xi - zi;
      else zi = -xi - yi;

      int q = xi;
      int r = zi;

      long key = Pack(q, r);
      if (key == lastKey) continue;
      lastKey = key;

      if (!visitor(q, r)) return;
    }
  }

  void BuildLayout()
  {
    var centers = new List<Vector3>(4096);
    var qa = new List<int>(4096);
    var rb = new List<int>(4096);

    float s = _cellSize;
    int rMin = Mathf.FloorToInt((-_halfHeight) / (1.5f * s)) - 2;
    int rMax = Mathf.CeilToInt((_halfHeight) / (1.5f * s)) + 2;

    var tmpIndex = new Dictionary<long, int>(4096);

    for (int r = rMin; r <= rMax; r++)
    {
      float xOffset = 1.7320508075688772f * s * (r * 0.5f);
      float xMin = -_halfWidth - xOffset;
      float xMax = _halfWidth - xOffset;

      float qMinF = xMin / (1.7320508075688772f * s);
      float qMaxF = xMax / (1.7320508075688772f * s);

      int qMin = Mathf.FloorToInt(qMinF) - 2;
      int qMax = Mathf.CeilToInt(qMaxF) + 2;

      for (int q = qMin; q <= qMax; q++)
      {
        Vector3 c = AxialCenterWorld(q, r);
        float lx = c.x - _origin.x;
        float lz = c.z - _origin.z;

        if (lx < -_halfWidth || lx > _halfWidth) continue;
        if (lz < -_halfHeight || lz > _halfHeight) continue;

        long key = Pack(q, r);
        if (tmpIndex.ContainsKey(key)) continue;

        tmpIndex.Add(key, centers.Count);
        centers.Add(c);
        qa.Add(q);
        rb.Add(r);
      }
    }

    _centers = centers.ToArray();
    _a = qa.ToArray();
    _b = rb.ToArray();
    _tags = new NavCellTag[_centers.Length];
  }

  void RebuildIndexIfNeeded()
  {
    if (_index != null && _index.Count == CellCount) return;

    _index = new Dictionary<long, int>(Mathf.Max(CellCount * 2, 128));
    for (int i = 0; i < _centers.Length; i++)
      _index[Pack(_a[i], _b[i])] = i;
  }

  static long Pack(int q, int r)
  {
    unchecked { return ((long)q << 32) ^ (uint)r; }
  }

  public void AppendAdjacentCells(
  int q,
  int r,
  AppendOnly<GridNeighbor> outNeighbors)
  {
    outNeighbors.Append(new GridNeighbor(q + 1, r, 1f));
    outNeighbors.Append(new GridNeighbor(q - 1, r, 1f));
    outNeighbors.Append(new GridNeighbor(q, r + 1, 1f));
    outNeighbors.Append(new GridNeighbor(q, r - 1, 1f));
    outNeighbors.Append(new GridNeighbor(q + 1, r - 1, 1f));
    outNeighbors.Append(new GridNeighbor(q - 1, r + 1, 1f));
  }
}