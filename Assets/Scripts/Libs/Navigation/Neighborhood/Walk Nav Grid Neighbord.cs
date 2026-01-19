using System.Collections.Generic;
using UnityEngine;

public sealed class WalkNavGridNeighborhood : INavigationNeighborhood
{
  [field:SerializeField]
  public NavigationTag Tag { get; set; }
  public float CostPerStep = 1f;

  static readonly List<GridNeighbor> AdjScratch = new(capacity: 16);

  public void AppendNeighbors(Vector3 from, AppendOnly<Neighbor> neighbors)
  {
    var grid = NavGraph.Active;
    if (grid == null) return;

    if (!grid.TryWorldToCell(from, out int a, out int b))
      return;

    if (!(grid.TryGetTag(a, b, out var t) && t == NavCellTag.Walk))
      return;

    AdjScratch.Clear();
    grid.AppendAdjacentCells(a, b, new AppendOnly<GridNeighbor>(AdjScratch));

    for (int i = 0; i < AdjScratch.Count; i++)
    {
      var n = AdjScratch[i];
      if (!(grid.TryGetTag(n.A, n.B, out var nt) && nt == NavCellTag.Walk))
        continue;

      neighbors.Append(new Neighbor(
        grid.CellCenterWorld(n.A, n.B),
        CostPerStep * n.CostMul,
        Tag));
    }
  }
}