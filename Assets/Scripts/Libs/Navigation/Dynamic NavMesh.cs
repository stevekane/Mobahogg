using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Linq;

[RequireComponent(typeof(NavMeshSurface))]
class DynamicNavMesh : MonoBehaviour
{
  [SerializeField] Transform Start;
  [SerializeField] Transform End;
  [SerializeField] int HalfWidth = 64;
  [SerializeField] int HalfHeight = 40;
  [SerializeField] bool RenderGrid;
  [SerializeField] bool RenderChunks;
  [SerializeField, Range(2, 20)] int JumpDistCardinal = 7;
  [SerializeField] int JumpDistDiagonal => Mathf.RoundToInt(JumpDistCardinal / Mathf.Sqrt(2));

  bool[,] Cells = new bool[0, 0];
  Vector3[,] CellPositions = new Vector3[0, 0];
  List<RectBounds> Chunks = new();

  (int, int) WorldSpaceToIndex2D(Vector3 p) =>
    (Mathf.RoundToInt(p.x + HalfWidth), Mathf.RoundToInt(p.z + HalfHeight));

  Vector3 Index2DToWorldSpace(int i, int j) =>
    new Vector3(i, 0, j) + new Vector3(-HalfWidth, 0, -HalfHeight);

  Vector3 Index2DToWorldSpace((int, int) tuple) =>
    Index2DToWorldSpace(tuple.Item1, tuple.Item2);

  void FixedUpdate()
  {
    GetComponent<NavMeshSurface>().BuildNavMesh();
    var width = HalfWidth * 2;
    var height = HalfHeight * 2;
    Cells = new bool[width, height];
    CellPositions = new Vector3[width, height];
    for (var i = 0; i < width; i++)
    {
      for (var j = 0; j < height; j++)
      {
        var origin = Index2DToWorldSpace(i, j);
        var color = Color.green;
        color.a = 0.5f;
        Gizmos.color = color;
        Cells[i, j] = NavMesh.SamplePosition(origin, out var _, 0.5f, NavMesh.AllAreas);
        CellPositions[i, j] = origin;
      }
    }
    Chunks = GridRectChunker.Chunkify(Cells);
  }

  void OnDrawGizmos()
  {
    var width = Cells.GetLength(0);
    var height = Cells.GetLength(1);
    if (RenderGrid)
    {
      for (var i = 0; i < width; i++)
      {
        for (var j = 0; j < height; j++)
        {
          if (Cells[i, j])
          {
            var origin = Index2DToWorldSpace(i, j);
            var color = Color.green;
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawWireCube(origin, Vector3.one);
          }
        }
      }
    }
    if (RenderChunks)
    {
      Random.InitState(0);
      foreach (var chunk in Chunks)
      {
        var min = Index2DToWorldSpace(chunk.MinX, chunk.MinY);
        var max = Index2DToWorldSpace(chunk.MaxX, chunk.MaxY);
        var center = 0.5f * (max + min);
        var size = new Vector3(chunk.Width, 0, chunk.Height);
        Gizmos.color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
        Gizmos.DrawWireCube(center, size);
      }
    }
    var startPos = Start.position;
    var endPos = End.position;
    var startingCell = WorldSpaceToIndex2D(startPos);
    var endingCell = WorldSpaceToIndex2D(endPos);
    Debug.DrawRay(startPos, Vector3.up, Color.green);
    Debug.DrawRay(endPos, Vector3.up, Color.red);
    var path = AStarGrid.StripColinearWalkingPoints(AStarGrid.FindPath(
      Cells,
      CellPositions,
      startingCell,
      endingCell,
      JumpDistCardinal,
      JumpDistDiagonal));
    Gizmos.color = Color.white;
    Gizmos.DrawLineStrip(path.Select(n => Index2DToWorldSpace(n.Index)).ToArray(), false);
    foreach (var point in path)
    {
      Gizmos.color = point.Tag switch
      {
        ActionTag.Jump => Color.magenta,
        _ => Color.white
      };
      Gizmos.DrawRay(point.Position, Vector3.up);
    }
  }
}

public readonly struct RectBounds
{
  public readonly int MinX, MinY;
  public readonly int MaxX, MaxY; // inclusive

  public int Width => MaxX - MinX + 1;
  public int Height => MaxY - MinY + 1;

  public RectBounds(int minX, int minY, int maxX, int maxY)
  {
    MinX = minX; MinY = minY;
    MaxX = maxX; MaxY = maxY;
  }

  public override string ToString() => $"[{MinX},{MinY}]..[{MaxX},{MaxY}]";
}

public static class GridRectChunker
{
  public static List<RectBounds> Chunkify(bool[,] grid)
  {
    var w = grid.GetLength(0);
    var h = grid.GetLength(1);
    var covered = new bool[w, h];
    var rects = new List<RectBounds>();

    for (int y = 0; y < h; y++)
      for (int x = 0; x < w; x++)
      {
        if (!grid[x, y] || covered[x, y])
          continue;

        int minX = x, minY = y;
        int maxX = x;
        while (maxX + 1 < w && grid[maxX + 1, y] && !covered[maxX + 1, y])
          maxX++;
        int maxY = y;
        while (maxY + 1 < h)
        {
          int ny = maxY + 1;
          bool ok = true;
          for (int xx = minX; xx <= maxX; xx++)
          {
            if (!grid[xx, ny] || covered[xx, ny])
            {
              ok = false;
              break;
            }
          }
          if (!ok) break;
          maxY++;
        }
        for (int yy = minY; yy <= maxY; yy++)
          for (int xx = minX; xx <= maxX; xx++)
            covered[xx, yy] = true;
        rects.Add(new RectBounds(minX, minY, maxX, maxY));
      }

    return rects;
  }
}

public enum ActionTag
{
  Walk,
  Jump
}

public readonly struct ActionPlanNode
{
  public readonly ActionTag Tag;
  public readonly (int, int) Index;
  public readonly Vector3 Position;

  public ActionPlanNode(ActionTag tag, (int, int) index, Vector3 position)
  {
    Tag = tag;
    Index = index;
    Position = position;
  }
}

public static class AStarGrid
{
  public static List<ActionPlanNode> FindPath(
    bool[,] walkable,
    Vector3[,] positions,
    (int x, int y) start,
    (int x, int y) goal,
    int JumpDistCardinal,
    int JumpDistDiagonal)
  {
    int w = walkable.GetLength(0);
    int h = walkable.GetLength(1);

    bool InBounds((int x, int y) p)
      => (uint)p.x < (uint)w && (uint)p.y < (uint)h;

    if (!InBounds(start) || !InBounds(goal)) return new();
    if (!walkable[start.x, start.y] || !walkable[goal.x, goal.y]) return new();

    const int WalkCost = 1;
    int JumpCost = JumpDistCardinal;

    int Heuristic((int x, int y) a)
      => Mathf.Abs(a.x - goal.x) + Mathf.Abs(a.y - goal.y);

    var open = new List<(int x, int y)> { start };
    var openSet = new HashSet<(int x, int y)> { start };

    // Store BOTH parent and the action tag used to reach this node from its parent.
    var cameFrom = new Dictionary<(int x, int y), ((int x, int y) parent, ActionTag tag)>();
    var gScore = new Dictionary<(int x, int y), int>
    {
      [start] = 0
    };

    while (open.Count > 0)
    {
      int bestIndex = 0;
      var current = open[0];
      int bestF = gScore[current] + Heuristic(current);

      for (int i = 1; i < open.Count; i++)
      {
        var n = open[i];
        int f = gScore[n] + Heuristic(n);
        if (f < bestF)
        {
          bestF = f;
          bestIndex = i;
          current = n;
        }
      }

      open.RemoveAt(bestIndex);
      openSet.Remove(current);

      if (current == goal)
        return Reconstruct(cameFrom, positions, current);

      int gHere = gScore[current];

      void TryJumpTo((int x, int y) from, (int x, int y) candidate, int jumpCost)
      {
        if (!InBounds(candidate)) return;
        if (!walkable[candidate.x, candidate.y]) return;

        var startPos = positions[from.x, from.y];
        var endPos = positions[candidate.x, candidate.y];

        if (NavMesh.Raycast(startPos, endPos, out var _, NavMesh.AllAreas))
          Try(candidate, jumpCost, ActionTag.Jump);
      }

      void Try((int x, int y) nb, int stepCost, ActionTag tag)
      {
        if (!InBounds(nb)) return;
        if (!walkable[nb.x, nb.y]) return;

        int tentativeG = gHere + stepCost;

        if (!gScore.TryGetValue(nb, out int oldG) || tentativeG < oldG)
        {
          cameFrom[nb] = (current, tag);
          gScore[nb] = tentativeG;
          if (openSet.Add(nb))
            open.Add(nb);
        }
      }

      Try((current.x + 1, current.y), WalkCost, ActionTag.Walk);
      Try((current.x - 1, current.y), WalkCost, ActionTag.Walk);
      Try((current.x, current.y + 1), WalkCost, ActionTag.Walk);
      Try((current.x, current.y - 1), WalkCost, ActionTag.Walk);
      Try((current.x + 1, current.y + 1), WalkCost, ActionTag.Walk);
      Try((current.x + 1, current.y - 1), WalkCost, ActionTag.Walk);
      Try((current.x - 1, current.y + 1), WalkCost, ActionTag.Walk);
      Try((current.x - 1, current.y - 1), WalkCost, ActionTag.Walk);

      TryJumpTo(current, (current.x + JumpDistCardinal, current.y), JumpCost);
      TryJumpTo(current, (current.x - JumpDistCardinal, current.y), JumpCost);
      TryJumpTo(current, (current.x, current.y + JumpDistCardinal), JumpCost);
      TryJumpTo(current, (current.x, current.y - JumpDistCardinal), JumpCost);
      TryJumpTo(current, (current.x + JumpDistDiagonal, current.y + JumpDistDiagonal), JumpCost);
      TryJumpTo(current, (current.x + JumpDistDiagonal, current.y - JumpDistDiagonal), JumpCost);
      TryJumpTo(current, (current.x - JumpDistDiagonal, current.y + JumpDistDiagonal), JumpCost);
      TryJumpTo(current, (current.x - JumpDistDiagonal, current.y - JumpDistDiagonal), JumpCost);
    }

    return new();
  }

  static List<ActionPlanNode> Reconstruct(
    Dictionary<(int x, int y), ((int x, int y) parent, ActionTag tag)> cameFrom,
    Vector3[,] positions,
    (int x, int y) current)
  {
    var nodes = new List<ActionPlanNode>();
    while (true)
    {
      ActionTag tag = ActionTag.Walk;
      if (cameFrom.TryGetValue(current, out var prev))
        tag = prev.tag;

      nodes.Add(new ActionPlanNode(tag, (current.x, current.y), positions[current.x, current.y]));

      if (!cameFrom.TryGetValue(current, out var info))
        break; // reached start (no parent)

      current = info.parent;
    }

    nodes.Reverse();
    if (nodes.Count > 0)
      nodes[0] = new ActionPlanNode(ActionTag.Walk, nodes[0].Index, nodes[0].Position);

    return nodes;
  }

  public static List<ActionPlanNode> StripColinearWalkingPoints(List<ActionPlanNode> path)
  {
    if (path == null || path.Count <= 2) return path ?? new();

    // Keep endpoints, remove interior nodes that are:
    // - tagged Walk
    // - and lie on a straight line between prev and next (grid colinear)
    var result = new List<ActionPlanNode>(path.Count);
    result.Add(path[0]);

    for (int i = 1; i < path.Count - 1; i++)
    {
      var prev = path[i - 1];
      var cur = path[i];
      var next = path[i + 1];

      if (cur.Tag != ActionTag.Walk)
      {
        result.Add(cur);
        continue;
      }

      var (ax, ay) = prev.Index;
      var (bx, by) = cur.Index;
      var (cx, cy) = next.Index;

      int abx = bx - ax;
      int aby = by - ay;
      int bcx = cx - bx;
      int bcy = cy - by;

      // Normalize direction to -1/0/1 per axis (handles 8-neighbor steps)
      abx = abx == 0 ? 0 : (abx > 0 ? 1 : -1);
      aby = aby == 0 ? 0 : (aby > 0 ? 1 : -1);
      bcx = bcx == 0 ? 0 : (bcx > 0 ? 1 : -1);
      bcy = bcy == 0 ? 0 : (bcy > 0 ? 1 : -1);

      bool colinear = (abx == bcx) && (aby == bcy);

      if (!colinear)
        result.Add(cur);
    }

    result.Add(path[path.Count - 1]);
    return result;
  }
}