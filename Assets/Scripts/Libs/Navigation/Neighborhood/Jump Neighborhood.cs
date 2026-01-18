using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class JumpNeighborhood : INavigationNeighborhood
{
  [Min(1)]
  public float MaxJumpDistance = 5;

  [field: SerializeField]
  public NavigationTag Tag { get; set; }

  public int Neighbors(Vector3 from, Neighbor[] buffer, int offset)
  {
    const float NAVMESH_SAMPLE_DISTANCE = 0.25f;
    const int NAVMESH_AREA_MASK = NavMesh.AllAreas;
    int count = 0;
    foreach (var direction in Directions.Octal)
    {
      Vector3 target = from + MaxJumpDistance * direction;
      bool onNavMesh = NavMesh.SamplePosition(target, out var hit, NAVMESH_SAMPLE_DISTANCE, NAVMESH_AREA_MASK);
      bool noDirectPath = NavMesh.Raycast(from, target, out var _, NAVMESH_AREA_MASK);
      if (onNavMesh && noDirectPath)
      {
        buffer[offset + count] = new Neighbor(hit.position, MaxJumpDistance, Tag);
        count++;
      }
    }
    return count;
  }
}