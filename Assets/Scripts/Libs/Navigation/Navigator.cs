using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Navigator : MonoBehaviour
{
  [SerializeField] Transform Target;
  [SerializeField] List<NavigationTag> StringPullTags;
  [SerializeField] PolymorphicList<INavigationNeighborhood> NavigationNeighborhoods;

  readonly List<NavigationNode> Nodes = new(capacity: 64);

  /*
  TODO: think about sane options for supporting sampling larger regions ( say for example by radius )
        this would allow jumping off the diagonals / cardinals which may allow that system to find
        better paths. This problem is less relevant for walking nodes but for longer-range scans
        it becomes weirder
  TODO: consider if raw access to the navmesh should be a thing. Maybe systems should interact with
        the NavigationSystem allowing more abstractions / optimizations than raw acces to the NavMesh
        provides
  TODO: add ballistic collision-checking to jumping to eliminate candidate nodes and capture the
        idea that jumping through blockers makes no sense
  TODO: consider if a hextile discretization would more naturally suport radial-like queries
  */

  void OnDrawGizmos()
  {
    var from = transform.position;
    var to = Target ? Target.transform.position : Vector3.zero;
    NavigationSystem.CalculatePath(
      from,
      to,
      Nodes,
      NavigationNeighborhoods.AsList(),
      goalRadius: 1,
      quantize: 1);
    PathSmoothing.StringPull(Nodes, n => StringPullTags.Contains(n.Tag));
    Gizmos.color = Color.green;
    Gizmos.DrawLineStrip(Nodes.Select(n => n.Position).ToArray(), looped: false);
  }
}