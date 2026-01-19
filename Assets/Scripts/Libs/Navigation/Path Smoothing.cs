using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class PathSmoothing
{
  public static int StringPull(
    List<NavigationNode> nodes,
    List<NavigationTag> pullTags,
    int areaMask = NavMesh.AllAreas)
  {
    int n = nodes.Count;
    if (n <= 2) return n;
    if (pullTags.Count == 0) return n;

    bool CanDirect(Vector3 a, Vector3 b)
      => !NavMesh.Raycast(a, b, out var _, areaMask);

    int write = 0;
    nodes[write++] = nodes[0];

    int i = 1;
    while (i < n)
    {
      if (!pullTags.Contains(nodes[i].Tag))
      {
        nodes[write++] = nodes[i++];
        continue;
      }

      Vector3 anchor = nodes[write - 1].Position;
      int best = i;
      int j = i;

      while (j < n)
      {
        if (!pullTags.Contains(nodes[j].Tag))
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
