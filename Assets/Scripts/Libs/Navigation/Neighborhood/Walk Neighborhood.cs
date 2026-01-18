using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class WalkNeighborhood : INavigationNeighborhood
{
  [field: SerializeField]
  public NavigationTag Tag { get; set; }

  public void AppendNeighbors(Vector3 from, AppendOnly<Neighbor> neighbors)
  {
    const float WALK_DISTANCE = 1;
    const float WALK_COST = 1;
    const float NAVMESH_SAMPLE_DISTANCE = 0.25f;
    const int NAVMESH_AREA_MARK = NavMesh.AllAreas;
    foreach (var direction in Directions.Octal)
    {
      Vector3 target = from + WALK_DISTANCE * direction;
      if (NavMesh.SamplePosition(target, out var hit, NAVMESH_SAMPLE_DISTANCE, NAVMESH_AREA_MARK))
      {
        var cost = direction.magnitude * WALK_COST;
        neighbors.Append(new(hit.position, cost, Tag));
      }
    }
  }
}