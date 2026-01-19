using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Navigator : MonoBehaviour
{
  [SerializeField] Transform Target;
  [SerializeField] List<NavigationTag> StringPullTags;
  [SerializeField] PolymorphicList<INavigationNeighborhood> NavigationNeighborhoods;
  [SerializeField] bool DoNavigation = true;
  [SerializeField] bool Debug;

  readonly List<NavigationNode> Nodes = new(capacity: 128);

  /*
  TODO: think about sane options for supporting sampling larger regions ( say for example by radius )
        this would allow jumping off the diagonals / cardinals which may allow that system to find
        better paths. This problem is less relevant for walking nodes but for longer-range scans
        it becomes weirder
  TODO: consider if raw access to the navmesh should be a thing. Maybe systems should interact with
        the NavigationSystem allowing more abstractions / optimizations than raw acces to the NavMesh
        provides
  TODO: consider if a hextile discretization would more naturally suport radial-like queries
  */

  void FixedUpdate()
  {
    if (!DoNavigation)
      return;
    var from = transform.position;
    var to = Target ? Target.transform.position : Vector3.zero;
    NavigationSystem.CalculatePath(
      from,
      to,
      Nodes,
      NavigationNeighborhoods.AsList(),
      goalRadius: 1,
      quantize: 1);
    PathSmoothing.StringPull(Nodes, StringPullTags);
  }

  void OnDrawGizmos()
  {
    if (!Debug)
      return;
    Gizmos.color = Color.green;
    for (var i = 1; i < Nodes.Count; i++)
    {
      var p0 = Nodes[i-1].Position;
      var p1 = Nodes[i+0].Position;
      Gizmos.DrawLine(p0, p1);
    }
  }
}