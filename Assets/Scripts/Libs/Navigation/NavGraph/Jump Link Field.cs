using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct JumpEdge
{
  public readonly int FromA;
  public readonly int FromB;
  public readonly int ToA;
  public readonly int ToB;
  public readonly Vector2 DirXZ;
  public readonly float Distance;

  public JumpEdge(int fromA, int fromB, int toA, int toB, Vector2 dirXZ, float distance)
  {
    FromA = fromA;
    FromB = fromB;
    ToA = toA;
    ToB = toB;
    DirXZ = dirXZ;
    Distance = distance;
  }
}

public enum JumpDirections
{
  Octal = 8,
  Dodec = 12,
  Sixteen = 16,
}

public sealed class JumpLinkField
{
  public readonly float MaxJumpDistance;
  public readonly JumpDirections Directions;

  readonly Dictionary<long, int> _map; // (a,b)->cellIndex mapping in enumeration order
  readonly int[] _head;                // head per cellIndex
  readonly int[] _next;                // next edge index
  readonly JumpEdge[] _edges;
  readonly int _edgeCount;

  public int EdgeCount => _edgeCount;

  JumpLinkField(
    float maxJumpDistance,
    JumpDirections dirs,
    Dictionary<long, int> map,
    int[] head,
    int[] next,
    JumpEdge[] edges,
    int edgeCount)
  {
    MaxJumpDistance = maxJumpDistance;
    Directions = dirs;
    _map = map;
    _head = head;
    _next = next;
    _edges = edges;
    _edgeCount = edgeCount;
  }

  public bool TryGetCellIndex(int a, int b, out int cellIndex)
  {
    return _map.TryGetValue(Pack(a, b), out cellIndex);
  }

  public void ForEachFromIndex(int fromCellIndex, Func<JumpEdge, bool> visitor)
  {
    for (int e = _head[fromCellIndex]; e != -1; e = _next[e])
      if (!visitor(_edges[e])) return;
  }

  static long Pack(int a, int b)
  {
    unchecked { return ((long)a << 32) ^ (uint)b; }
  }

  static float Heuristic2D(Vector3 a, Vector3 b)
  {
    var d = b - a;
    d.y = 0f;
    return d.magnitude;
  }

  public static JumpLinkField Build(
    INavGrid grid,
    float maxJumpDistance,
    JumpDirections dirs = JumpDirections.Octal,
    float minCandidateDistance = 0.01f)
  {
    float R = Mathf.Max(0f, maxJumpDistance);
    int k = (int)dirs;
    var dirTable = BuildDirTable(k);

    // Build mapping from (a,b)->cellIndex in enumeration order.
    var map = new Dictionary<long, int>(grid.CellCount * 2);
    int cellIndex = 0;

    grid.ForEachCell((a, b, center, tag) =>
    {
      map[Pack(a, b)] = cellIndex++;
      return true;
    });

    var head = new int[grid.CellCount];
    for (int i = 0; i < head.Length; i++) head[i] = -1;

    // Conservative edge cap.
    int maxEdges = Mathf.Max(1, grid.CellCount * k);
    var edges = new JumpEdge[maxEdges];
    var next = new int[maxEdges];
    int edgeCount = 0;

    int fromIdx = 0;
    grid.ForEachCell((a, b, c, tag) =>
    {
      if (tag != NavCellTag.Walk) { fromIdx++; return true; }

      // only from boundary-adjacent walk cells
      bool boundary = false;
      for (int i = 0; i < k; i++)
      {
        Vector2 d = dirTable[i];
        Vector3 npos = c + new Vector3(d.x, 0f, d.y) * grid.CellSize;

        if (grid.TryWorldToCell(npos, out int na, out int nb) && grid.TryGetTag(na, nb, out var nt))
        {
          if (nt == NavCellTag.Void) { boundary = true; break; }
        }
        else
        {
          boundary = true;
          break;
        }
      }
      if (!boundary) { fromIdx++; return true; }

      // For each direction, require immediate void then first walk re-entry
      for (int i = 0; i < k; i++)
      {
        Vector2 d = dirTable[i];
        Vector3 dir3 = new Vector3(d.x, 0f, d.y);

        Vector3 step1 = c + dir3 * grid.CellSize;
        if (!(grid.TryWorldToCell(step1, out int s1a, out int s1b) && grid.TryGetTag(s1a, s1b, out var t1) && t1 == NavCellTag.Void))
          continue;

        bool sawVoid = false;
        bool landed = false;
        int landA = 0, landB = 0;
        float landDist = 0f;

        grid.RayWalk(c, dir3, R, (aa, bb) =>
        {
          Vector3 cc = grid.CellCenterWorld(aa, bb);
          float dist = Heuristic2D(c, cc);
          if (dist < minCandidateDistance) return true;

          if (!grid.TryGetTag(aa, bb, out var tt)) tt = NavCellTag.Void;

          if (!sawVoid)
          {
            if (tt == NavCellTag.Void) { sawVoid = true; return true; }
            return true;
          }

          if (tt == NavCellTag.Walk)
          {
            landed = true;
            landA = aa;
            landB = bb;
            landDist = dist;
            return false;
          }

          return true;
        });

        if (!landed) continue;

        if (edgeCount >= edges.Length)
        {
          int newCap = edges.Length * 2;
          Array.Resize(ref edges, newCap);
          Array.Resize(ref next, newCap);
        }

        edges[edgeCount] = new JumpEdge(a, b, landA, landB, d, landDist);
        next[edgeCount] = head[fromIdx];
        head[fromIdx] = edgeCount;
        edgeCount++;
      }

      fromIdx++;
      return true;
    });

    Array.Resize(ref edges, edgeCount);
    Array.Resize(ref next, edgeCount);

    return new JumpLinkField(R, dirs, map, head, next, edges, edgeCount);
  }

  static Vector2[] BuildDirTable(int k)
  {
    var dirs = new Vector2[k];
    float step = 2f * Mathf.PI / k;
    float baseAngle = 15f * Mathf.Deg2Rad;
    for (int i = 0; i < k; i++)
    {
      float a = baseAngle + step * i;
      dirs[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
    }
    return dirs;
  }
}