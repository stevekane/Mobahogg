using System.Collections.Generic;
using UnityEngine;

public static class NavGraph
{
  public static JumpLinkField JumpField { get; private set; }
  public static CartesianNavGraph Cartesian { get; private set; }
  public static HexagonalNavGraph Hex { get; private set; }
  public static INavGrid Active { get; private set; }

  public static void Set(CartesianNavGraph cartesian, HexagonalNavGraph hex, INavGrid active = null)
  {
    Cartesian = cartesian;
    Hex = hex;
    Active = active ?? (INavGrid)cartesian ?? (INavGrid)hex;
  }

  public static void SetActiveCartesian() => Active = Cartesian;
  public static void SetActiveHex() => Active = Hex;

  // Main-thread scratch storage. Allocates only if capacities are exceeded.
  static readonly List<int> A = new(capacity: 4096);
  static readonly List<int> B = new(capacity: 4096);
  static readonly List<Vector3> Positions = new(capacity: 4096);
  static readonly List<float> G = new(capacity: 4096);
  static readonly List<float> F = new(capacity: 4096);
  static readonly List<int> Parent = new(capacity: 4096);
  static readonly List<NavigationTag> ArriveTag = new(capacity: 4096);
  static readonly List<byte> State = new(capacity: 4096); // 0=unseen,1=open,2=closed

  static readonly Dictionary<long, int> IndexByCell = new(capacity: 8192);
  static readonly List<Neighbor> NeighborScratch = new(capacity: 512);

  static readonly MinHeap Open = new(capacity: 4096);

  public static void RebuildJumpField(float maxJumpDistance, JumpDirections dirs = JumpDirections.Octal)
  {
    if (Active == null)
    {
      JumpField = null;
      return;
    }
    JumpField = JumpLinkField.Build(Active, maxJumpDistance, dirs);
  }

  static float Heuristic(Vector3 a, Vector3 b)
  {
    var d = b - a;
    d.y = 0f;
    return d.magnitude;
  }

  static long Pack(int a, int b)
  {
    unchecked { return ((long)a << 32) ^ (uint)b; }
  }

  static int AddNode(int a, int b, Vector3 p, NavigationTag tag, int parentIndex, float gScore, Vector3 goalPos)
  {
    int idx = Positions.Count;

    A.Add(a);
    B.Add(b);

    Positions.Add(p);
    G.Add(gScore);

    float fScore = gScore + Heuristic(p, goalPos);
    F.Add(fScore);

    Parent.Add(parentIndex);
    ArriveTag.Add(tag);
    State.Add(1);

    return idx;
  }

  static void RelaxNode(int idx, int parentIndex, float gScore, NavigationTag tag, Vector3 goalPos)
  {
    G[idx] = gScore;

    float fScore = gScore + Heuristic(Positions[idx], goalPos);
    F[idx] = fScore;

    Parent[idx] = parentIndex;
    ArriveTag[idx] = tag;

    if (State[idx] == 2)
      State[idx] = 1; // re-open
  }

  static int Reconstruct(int endIndex, List<NavigationNode> outNodes)
  {
    int cur = endIndex;
    while (cur >= 0)
    {
      outNodes.Add(new NavigationNode(Positions[cur], ArriveTag[cur]));
      cur = Parent[cur];
    }

    outNodes.Reverse();
    return outNodes.Count;
  }

  static bool TrySnapToWalk(INavGrid grid, Vector3 world, out int a, out int b, out Vector3 snapped)
  {
    // 1) direct cell
    if (grid.TryWorldToCell(world, out a, out b) && grid.TryGetTag(a, b, out var t) && t == NavCellTag.Walk)
    {
      snapped = grid.CellCenterWorld(a, b);
      return true;
    }

    // 2) cheap local search: expand 1-ring..3-ring in cartesian space by sampling nearby world offsets.
    // (This avoids requiring a separate "nearest walk" acceleration structure.)
    // NOTE: for hex this is still fine because TryWorldToCell maps to nearest axial cell.
    const int RINGS = 3;
    float step = grid.CellSize;

    for (int r = 1; r <= RINGS; r++)
    {
      float d = r * step;
      for (int i = 0; i < 8; i++)
      {
        Vector3 off = Directions.Octal[i] * d;
        Vector3 p = world + off;

        if (grid.TryWorldToCell(p, out a, out b) && grid.TryGetTag(a, b, out t) && t == NavCellTag.Walk)
        {
          snapped = grid.CellCenterWorld(a, b);
          return true;
        }
      }
    }

    snapped = default;
    a = b = 0;
    return false;
  }

  public static int CalculatePath(
    Vector3 from,
    Vector3 to,
    List<NavigationNode> nodes,
    List<INavigationNeighborhood> neighborhoods,
    float goalRadius = 1f,
    int maxIterations = 20000)
  {
    nodes.Clear();
    if (neighborhoods == null || neighborhoods.Count == 0) return 0;

    var grid = Active;
    if (grid == null) return 0;

    if (!TrySnapToWalk(grid, from, out int startA, out int startB, out var startPos)) return 0;
    if (!TrySnapToWalk(grid, to, out int goalA, out int goalB, out var goalPos)) return 0;

    float goalRad2 = goalRadius * goalRadius;
    if ((startPos - goalPos).sqrMagnitude <= goalRad2)
    {
      nodes.Add(new NavigationNode(startPos, default));
      return 1;
    }

    Open.Clear();
    IndexByCell.Clear();

    A.Clear();
    B.Clear();
    Positions.Clear();
    G.Clear();
    F.Clear();
    Parent.Clear();
    ArriveTag.Clear();
    State.Clear();

    int startIndex = AddNode(startA, startB, startPos, default, parentIndex: -1, gScore: 0f, goalPos);
    IndexByCell[Pack(startA, startB)] = startIndex;
    Open.Push(startIndex, F[startIndex]);

    int iters = 0;
    while (Open.Count > 0 && iters++ < maxIterations)
    {
      int current = Open.PopMin();

      if (State[current] == 2)
        continue; // stale heap entry

      State[current] = 2;

      Vector3 cp = Positions[current];
      if ((cp - goalPos).sqrMagnitude <= goalRad2)
        return Reconstruct(current, nodes);

      NeighborScratch.Clear();
      var appendOnly = new AppendOnly<Neighbor>(NeighborScratch);

      // Neighborhoods produce neighbor *positions* in world space.
      // For grid walk, these will be other cell centers.
      for (int n = 0; n < neighborhoods.Count; n++)
        neighborhoods[n].AppendNeighbors(cp, appendOnly);

      for (int i = 0; i < NeighborScratch.Count; i++)
      {
        var nb = NeighborScratch[i];

        // Quantize neighbor position to a grid cell and reject void.
        if (!grid.TryWorldToCell(nb.Position, out int na, out int nbB)) continue;
        if (!(grid.TryGetTag(na, nbB, out var tag) && tag == NavCellTag.Walk)) continue;

        Vector3 np = grid.CellCenterWorld(na, nbB);

        float tentativeG = G[current] + nb.Cost;
        long key = Pack(na, nbB);

        if (IndexByCell.TryGetValue(key, out int idx))
        {
          if (tentativeG >= G[idx])
            continue;

          RelaxNode(idx, current, tentativeG, nb.Tag, goalPos);
          Open.Push(idx, F[idx]);
        }
        else
        {
          int nextIndex = AddNode(na, nbB, np, nb.Tag, current, tentativeG, goalPos);
          IndexByCell[key] = nextIndex;
          Open.Push(nextIndex, F[nextIndex]);
        }
      }
    }

    return 0;
  }
}