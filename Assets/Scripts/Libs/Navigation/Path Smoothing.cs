using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class PathSmoothing
{
  public static int StringPull(
    List<NavigationNode> nodes,
    Func<NavigationNode, bool> isPullable,
    int areaMask = NavMesh.AllAreas)
  {
    int n = nodes.Count;
    if (n <= 2) return n;
    if (isPullable == null) return n;

    bool CanDirect(Vector3 a, Vector3 b)
      => !NavMesh.Raycast(a, b, out var _, areaMask);

    int write = 0;
    nodes[write++] = nodes[0];

    int i = 1;
    while (i < n)
    {
      // Hard barrier node: keep it, do not attempt to pull across it.
      if (!isPullable(nodes[i]))
      {
        nodes[write++] = nodes[i++];
        continue;
      }

      // We are in a run of pullable nodes. Anchor from last kept node.
      Vector3 anchor = nodes[write - 1].Position;

      // Choose farthest pullable node reachable from anchor, without crossing a barrier.
      int best = i;
      int j = i;

      while (j < n)
      {
        if (!isPullable(nodes[j]))
          break;

        if (CanDirect(anchor, nodes[j].Position))
        {
          best = j;
          j++;
          continue;
        }

        break;
      }

      nodes[write++] = nodes[best];
      i = best + 1;
    }

    if (write < n)
      nodes.RemoveRange(write, n - write);

    return nodes.Count;
  }
}
