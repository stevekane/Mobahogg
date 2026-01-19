using UnityEngine;
using UnityEngine.AI;

public static class NavGraphGenerators
{
  public static CartesianNavGraph BuildCartesianFromNavMesh(
    Vector3 origin,
    float cellSize,
    float halfWidth,
    float halfHeight,
    int areaMask = NavMesh.AllAreas)
  {
    var g = new CartesianNavGraph(cellSize, origin, halfWidth, halfHeight);

    float sampleDist = Mathf.Max(0.05f, cellSize * 0.45f);

    for (int z = 0; z < g.Height; z++)
    for (int x = 0; x < g.Width; x++)
    {
      Vector3 p = g.CellCenterWorld(x, z);
      bool ok = NavMesh.SamplePosition(p, out var hit, sampleDist, areaMask);
      g.TagRef(x, z) = ok ? NavCellTag.Walk : NavCellTag.Void;
    }

    return g;
  }

  public static HexagonalNavGraph BuildHexFromNavMesh(
    Vector3 origin,
    float cellSize,
    float halfWidth,
    float halfHeight,
    int areaMask = NavMesh.AllAreas)
  {
    var g = new HexagonalNavGraph(cellSize, origin, halfWidth, halfHeight);

    float sampleDist = Mathf.Max(0.05f, cellSize * 0.5f);

    for (int i = 0; i < g.CellCount; i++)
    {
      Vector3 c = g.GetCenterAtIndex(i);
      bool ok = NavMesh.SamplePosition(c, out var hit, sampleDist, areaMask);
      g.SetTagAtIndex(i, ok ? NavCellTag.Walk : NavCellTag.Void);
    }

    return g;
  }
}