using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public readonly struct NavigationNode
{
  public readonly Vector3 Position;
  public readonly NavigationTag Tag;

  public NavigationNode(Vector3 position, NavigationTag tag)
  {
    Position = position;
    Tag = tag;
  }
}

public readonly struct Neighbor
{
  public readonly Vector3 Position;
  public readonly NavigationTag Tag;
  public readonly float Cost;

  public Neighbor(Vector3 position, float cost, NavigationTag tag)
  {
    Position = position;
    Cost = cost;
    Tag = tag;
  }
}

public static class NavigationSystem
{
  // Main-thread scratch storage. Allocates only if capacities are exceeded.
  static readonly List<Vector3> Positions = new(capacity: 4096);
  static readonly List<float> G = new(capacity: 4096);
  static readonly List<float> F = new(capacity: 4096);
  static readonly List<int> Parent = new(capacity: 4096);
  static readonly List<NavigationTag> ArriveTag = new(capacity: 4096);
  static readonly List<byte> State = new(capacity: 4096); // 0=unseen,1=open,2=closed

  static readonly Dictionary<Vector3Int, int> IndexByCell = new(capacity: 8192);
  static readonly List<Neighbor> NeighborScratch = new(capacity: 512);

  static readonly MinHeap Open = new(capacity: 4096);

  static bool TrySampleOnNavMesh(Vector3 p, out Vector3 onMesh)
  {
    if (NavMesh.SamplePosition(p, out var hit, 0.25f, NavMesh.AllAreas))
    {
      onMesh = hit.position;
      return true;
    }
    onMesh = default;
    return false;
  }

  static float Heuristic(Vector3 a, Vector3 b)
  {
    var d = b - a;
    d.y = 0f;
    return d.magnitude;
  }

  static Vector3Int QuantKey(Vector3 p, float q)
  {
    return new Vector3Int(
      Mathf.FloorToInt(p.x / q),
      Mathf.FloorToInt(p.y / q),
      Mathf.FloorToInt(p.z / q));
  }

  static void EnsureCapacity<T>(List<T> list, int cap)
  {
    if (list.Capacity < cap) list.Capacity = cap;
  }

  static void ResetSearch(int maxNodesHint)
  {
    Open.Clear();
    IndexByCell.Clear();

    Positions.Clear();
    G.Clear();
    F.Clear();
    Parent.Clear();
    ArriveTag.Clear();
    State.Clear();

    EnsureCapacity(Positions, maxNodesHint);
    EnsureCapacity(G, maxNodesHint);
    EnsureCapacity(F, maxNodesHint);
    EnsureCapacity(Parent, maxNodesHint);
    EnsureCapacity(ArriveTag, maxNodesHint);
    EnsureCapacity(State, maxNodesHint);
  }

  static int AddNode(Vector3 p, NavigationTag tag, int parentIndex, float gScore, Vector3 goalPos)
  {
    int idx = Positions.Count;

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

  public static int CalculatePath(
    Vector3 from,
    Vector3 to,
    List<NavigationNode> nodes,
    List<INavigationNeighborhood> neighborhoods,
    float goalRadius = 1f,
    float quantize = 1f,
    int maxNodes = 4096,
    int maxIterations = 20000)
  {
    nodes.Clear();
    if (neighborhoods.Count == 0) return 0;

    if (!TrySampleOnNavMesh(from, out var startPos)) return 0;
    if (!TrySampleOnNavMesh(to, out var goalPos)) return 0;

    float goalRad2 = goalRadius * goalRadius;
    if ((startPos - goalPos).sqrMagnitude <= goalRad2)
    {
      nodes.Add(new NavigationNode(startPos, default));
      return 1;
    }

    ResetSearch(maxNodes);

    int startIndex = AddNode(startPos, default, parentIndex: -1, gScore: 0f, goalPos);
    IndexByCell[QuantKey(startPos, quantize)] = startIndex;
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
      for (int n = 0; n < neighborhoods.Count; n++)
        neighborhoods[n].AppendNeighbors(cp, appendOnly);

      for (int i = 0; i < NeighborScratch.Count; i++)
      {
        var nb = NeighborScratch[i];

        if (!TrySampleOnNavMesh(nb.Position, out var np))
          continue;

        float tentativeG = G[current] + nb.Cost;
        var key = QuantKey(np, quantize);

        if (IndexByCell.TryGetValue(key, out int idx))
        {
          if (tentativeG >= G[idx])
            continue;

          RelaxNode(idx, current, tentativeG, nb.Tag, goalPos);
          Open.Push(idx, F[idx]);
        }
        else
        {
          if (Positions.Count >= maxNodes)
            continue;

          int nextIndex = AddNode(np, nb.Tag, current, tentativeG, goalPos);
          IndexByCell[key] = nextIndex;
          Open.Push(nextIndex, F[nextIndex]);
        }
      }
    }

    return 0;
  }
}