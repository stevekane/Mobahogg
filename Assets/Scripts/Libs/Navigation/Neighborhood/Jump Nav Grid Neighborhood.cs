using UnityEngine;

[System.Serializable]
public sealed class JumpNavGridNeighborhood : INavigationNeighborhood
{
  [field: SerializeField]
  public NavigationTag Tag { get; set; }

  [Min(1f)]
  public float MinJumpDistance = 0.5f;

  [Min(1f)]
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

    if (!field.TryGetCellIndex(a, b, out int idx))
      return;

    float minD = Mathf.Max(0f, MinJumpDistance);
    float maxD = Mathf.Max(minD, MaxJumpDistance);
    maxD = Mathf.Min(maxD, field.MaxJumpDistance);
    field.ForEachFromIndex(idx, e =>
    {
      float d = e.Distance;
      if (d < minD || d > maxD) return true;
      if (!(grid.TryGetTag(e.ToA, e.ToB, out var lt) && lt == NavCellTag.Walk))
        return true;
      Vector3 landing = grid.CellCenterWorld(e.ToA, e.ToB);
      float cost = d * CostPerUnitDistance + ExtraCost;
      neighbors.Append(new Neighbor(landing, cost, Tag));
      return true;
    });
  }
}