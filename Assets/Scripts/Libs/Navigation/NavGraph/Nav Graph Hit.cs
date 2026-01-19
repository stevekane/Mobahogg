using UnityEngine;

public readonly struct NavGraphHit
{
  public readonly bool Hit;
  public readonly Vector3 World;
  public readonly NavCellTag Tag;
  public readonly int A;
  public readonly int B;

  public NavGraphHit(bool hit, Vector3 world, NavCellTag tag, int a, int b)
  {
    Hit = hit;
    World = world;
    Tag = tag;
    A = a;
    B = b;
  }
}