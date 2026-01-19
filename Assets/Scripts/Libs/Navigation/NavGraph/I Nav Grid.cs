using System;
using UnityEngine;

public readonly struct GridNeighbor
{
  public readonly int A;
  public readonly int B;
  public readonly float CostMul;

  public GridNeighbor(int a, int b, float costMul)
  {
    A = a;
    B = b;
    CostMul = costMul;
  }
}

public interface INavGrid
{
  float CellSize { get; }
  Vector3 Origin { get; }
  float HalfWidth { get; }
  float HalfHeight { get; }
  int CellCount { get; }

  Vector3 CellCenterWorld(int a, int b);
  bool TryWorldToCell(Vector3 world, out int a, out int b);
  bool TryGetTag(int a, int b, out NavCellTag tag);
  void ForEachCell(Func<int, int, Vector3, NavCellTag, bool> visitor);
  void RayWalk(Vector3 worldFrom, Vector3 worldDir, float maxDistance, Func<int, int, bool> visitor);
  void AppendAdjacentCells(int a, int b, AppendOnly<GridNeighbor> outNeighbors);
}