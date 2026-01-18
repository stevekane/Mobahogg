using System;
using System.Collections.Generic;
using System.Buffers;
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
  public readonly float Cost;
  public readonly NavigationTag Tag;

  public Neighbor(Vector3 position, float cost, NavigationTag tag)
  {
    Position = position;
    Cost = cost;
    Tag = tag;
  }
}

public static class NavigationSystem
{
  // Caller-provided neighbor expansion. Fill into buffer, return count.
  public delegate int ExpandFunc(Vector3 from, Neighbor[] buffer);

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

  public static int CalculatePath(
    Vector3 from,
    Vector3 to,
    List<NavigationNode> nodes,
    List<INavigationNeighborhood> neighbors,
    float goalRadius = 1f,
    float quantize = 1f,
    int maxNodes = 4096,
    int maxIterations = 20000)
  {
    nodes.Clear();
    if (neighbors.Count == 0) return 0;

    if (!TrySampleOnNavMesh(from, out var startPos)) return 0;
    if (!TrySampleOnNavMesh(to, out var goalPos)) return 0;

    if ((startPos - goalPos).sqrMagnitude <= goalRadius * goalRadius)
    {
      nodes.Add(new NavigationNode(startPos, default));
      return 1;
    }

    var poolV3 = ArrayPool<Vector3>.Shared;
    var poolF = ArrayPool<float>.Shared;
    var poolI = ArrayPool<int>.Shared;
    var poolT = ArrayPool<NavigationTag>.Shared;
    var poolN = ArrayPool<Neighbor>.Shared;

    Vector3[] pos = poolV3.Rent(maxNodes);
    float[] g = poolF.Rent(maxNodes);
    float[] f = poolF.Rent(maxNodes);
    int[] parent = poolI.Rent(maxNodes);
    NavigationTag[] arriveTag = poolT.Rent(maxNodes);

    // Heap for open set, stores node indices
    int[] heap = poolI.Rent(maxNodes);
    int heapCount = 0;

    // Hash table mapping quantized key -> node index+1 (0 means empty)
    // Keep size as power-of-two-ish, at least 2x nodes.
    int hashSize = 1;
    while (hashSize < maxNodes * 2) hashSize <<= 1;
    int[] hashVals = poolI.Rent(hashSize);
    int[] state = poolI.Rent(maxNodes); // 0=unseen,1=open,2=closed

    // neighbor buffer: typically small (8 for walk)
    Neighbor[] neigh = poolN.Rent(64);

    try
    {
      Array.Clear(hashVals, 0, hashSize);
      Array.Clear(state, 0, maxNodes);

      int nodeCount = 0;

      int startIndex = AddOrGetNode(startPos, default, parentIndex: -1, gScore: 0f);
      Push(startIndex);

      int iters = 0;
      while (heapCount > 0 && iters++ < maxIterations)
      {
        int current = PopMin();
        if (state[current] == 2) continue; // stale heap entry
        state[current] = 2;

        Vector3 cp = pos[current];
        if ((cp - goalPos).sqrMagnitude <= goalRadius * goalRadius)
        {
          // Reconstruct into `nodes`
          return Reconstruct(current, nodes);
        }

        int nCount = 0;
        foreach (var neighborhood in neighbors)
        {
          nCount += neighborhood.Neighbors(cp, neigh, nCount);
        }

        for (int i = 0; i < nCount; i++)
        {
          var nb = neigh[i];

          // Snap neighbor to navmesh; if it can't be sampled, ignore
          if (!TrySampleOnNavMesh(nb.Position, out var np))
            continue;

          float tentativeG = g[current] + nb.Cost;

          int nbIndex = AddOrGetNode(np, nb.Tag, current, tentativeG);

          // If we didn't improve g-score, ignore
          if (nbIndex < 0)
            continue;

          // Add/update in open
          if (state[nbIndex] != 2)
          {
            state[nbIndex] = 1;
            Push(nbIndex);
          }
        }
      }

      return 0;

      // ----- Local helpers -----

      int AddOrGetNode(Vector3 p, NavigationTag tag, int parentIndex, float gScore)
      {
        if (nodeCount >= maxNodes) return -1;

        var key = QuantKey(p, quantize);
        int slot = FindSlot(key, hashVals, hashSize, pos, quantize);

        int stored = hashVals[slot] - 1;
        if (stored >= 0)
        {
          // Existing node: relax edge
          if (gScore < g[stored])
          {
            g[stored] = gScore;
            f[stored] = gScore + Heuristic(pos[stored], goalPos);
            parent[stored] = parentIndex;
            arriveTag[stored] = tag;
            return stored;
          }
          return -1; // no improvement
        }

        // New node
        int idx = nodeCount++;
        pos[idx] = p;
        g[idx] = gScore;
        f[idx] = gScore + Heuristic(p, goalPos);
        parent[idx] = parentIndex;
        arriveTag[idx] = tag;
        state[idx] = 1;
        hashVals[slot] = idx + 1;
        return idx;
      }

      float Heuristic(Vector3 a, Vector3 b)
      {
        // Simple: planar distance (time-optimal later)
        var d = b - a;
        d.y = 0f;
        return d.magnitude;
      }

      int Reconstruct(int endIndex, List<NavigationNode> outNodes)
      {
        // Build reversed, then reverse in-place by emitting into list and reversing.
        int count = 0;
        int cur = endIndex;
        while (cur >= 0)
        {
          // Tag means "action used to arrive here from parent"
          outNodes.Add(new NavigationNode(pos[cur], arriveTag[cur]));
          cur = parent[cur];
          count++;
          if (count > maxNodes) break;
        }
        outNodes.Reverse();

        // Start node has no "arrive action". Keep default tag; caller can ignore node[0].Tag.
        return outNodes.Count;
      }

      // ----- Heap (min by f) -----

      void Push(int node)
      {
        int i = heapCount++;
        heap[i] = node;
        while (i > 0)
        {
          int p = (i - 1) >> 1;
          if (f[heap[p]] <= f[heap[i]]) break;
          (heap[p], heap[i]) = (heap[i], heap[p]);
          i = p;
        }
      }

      int PopMin()
      {
        int root = heap[0];
        heapCount--;
        if (heapCount > 0)
        {
          heap[0] = heap[heapCount];
          int i = 0;
          while (true)
          {
            int l = i * 2 + 1;
            int r = l + 1;
            if (l >= heapCount) break;
            int s = (r < heapCount && f[heap[r]] < f[heap[l]]) ? r : l;
            if (f[heap[i]] <= f[heap[s]]) break;
            (heap[i], heap[s]) = (heap[s], heap[i]);
            i = s;
          }
        }
        return root;
      }

      // ----- Quantized hash -----

      static (int x, int y, int z) QuantKey(Vector3 p, float q)
      {
        // Use rounding; you can also use floor if you prefer.
        return (
          Mathf.RoundToInt(p.x / q),
          Mathf.RoundToInt(p.y / q),
          Mathf.RoundToInt(p.z / q)
        );
      }

      static int FindSlot(
        (int x, int y, int z) key,
        int[] table,
        int tableSize,
        Vector3[] pos,
        float q)
      {
        uint h = Hash(key);
        int mask = tableSize - 1;
        int slot = (int)(h & (uint)mask);

        // open addressing
        while (true)
        {
          int v = table[slot];
          if (v == 0) return slot;

          int idx = v - 1;
          var k2 = QuantKey(pos[idx], q);
          if (k2 == key) return slot;

          slot = (slot + 1) & mask;
        }
      }

      static uint Hash((int x, int y, int z) k)
      {
        // simple mix
        unchecked
        {
          uint h = 2166136261u;
          h = (h ^ (uint)k.x) * 16777619u;
          h = (h ^ (uint)k.y) * 16777619u;
          h = (h ^ (uint)k.z) * 16777619u;
          return h;
        }
      }
    }
    finally
    {
      poolV3.Return(pos, clearArray: false);
      poolF.Return(g, clearArray: false);
      poolF.Return(f, clearArray: false);
      poolI.Return(parent, clearArray: false);
      poolT.Return(arriveTag, clearArray: false);
      poolI.Return(heap, clearArray: false);
      poolI.Return(hashVals, clearArray: false);
      poolI.Return(state, clearArray: false);
      poolN.Return(neigh, clearArray: false);
    }
  }
}