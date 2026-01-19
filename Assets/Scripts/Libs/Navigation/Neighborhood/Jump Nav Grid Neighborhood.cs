using UnityEngine;

[System.Serializable]
public sealed class JumpNavGridNeighborhood : INavigationNeighborhood
{
  [field: SerializeField]
  public NavigationTag Tag { get; set; }

  [Min(0f)]
  public float MinJumpDistance = 0.5f;

  [Min(0f)]
  public float MaxJumpDistance = 10f;

  [Min(0f)]
  public float ExtraCost = 0f;

  [Min(0.0001f)]
  public float CostPerUnitDistance = 1f;

  public void AppendNeighbors(Vector3 from, AppendOnly<Neighbor> neighbors)
  {
    var grid = NavGraph.Active;
    var field = NavGraph.JumpField;
    if (grid == null || field == null) return;

    if (!grid.TryWorldToCell(from, out int a, out int b))
      return;

    if (!(grid.TryGetTag(a, b, out var t) && t == NavCellTag.Walk))
      return;

    if (!field.TryGetCellIndex(a, b, out int cellIndex))
      return;

    float minD = Mathf.Max(0f, MinJumpDistance);
    float maxD = Mathf.Max(minD, MaxJumpDistance);
    maxD = Mathf.Min(maxD, field.MaxJumpDistance);

    for (int ei = field.FirstEdgeIndex(cellIndex); ei != -1; ei = field.NextEdgeIndex(ei))
    {
      var e = field.GetEdge(ei);

      float d = e.Distance;
      if (d < minD || d > maxD)
        continue;

      // landing must still be walk (in case tags change dynamically)
      if (!(grid.TryGetTag(e.ToA, e.ToB, out var lt) && lt == NavCellTag.Walk))
        continue;

      Vector3 landing = grid.CellCenterWorld(e.ToA, e.ToB);
      float cost = d * CostPerUnitDistance + ExtraCost;

      neighbors.Append(new Neighbor(landing, cost, Tag));
    }
  }
}