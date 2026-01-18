using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class JumpNeighborhood : INavigationNeighborhood
{
  [Min(1)]
  public float MaxJumpDistance = 5;

  [field: SerializeField]
  public NavigationTag Tag { get; set; }

  public void AppendNeighbors(Vector3 from, AppendOnly<Neighbor> neighbors)
  {
    const float NAVMESH_SAMPLE_DISTANCE = 0.25f;
    const int NAVMESH_AREA_MASK = NavMesh.AllAreas;
    foreach (var direction in Directions.Octal)
    {
      Vector3 target = from + MaxJumpDistance * direction;
      bool onNavMesh = NavMesh.SamplePosition(target, out var hit, NAVMESH_SAMPLE_DISTANCE, NAVMESH_AREA_MASK);
      bool noDirectPath = NavMesh.Raycast(from, target, out var _, NAVMESH_AREA_MASK);
      bool raycastHit = Physics.Raycast(from, direction, maxDistance: MaxJumpDistance);
      if (onNavMesh && noDirectPath && !raycastHit)
      {
        neighbors.Append(new(hit.position, MaxJumpDistance, Tag));
      }
    }
  }
}