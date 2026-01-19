using System.Collections.Generic;
using UnityEngine;

public class Navigator : MonoBehaviour
{
  [SerializeField] Transform Target;
  [SerializeField] List<NavigationTag> StringPullTags;
  [SerializeField] PolymorphicList<INavigationNeighborhood> NavigationNeighborhoods;
  [SerializeField] bool DoNavigation = true;
  [SerializeField] bool Debug;

  readonly List<NavigationNode> Nodes = new(capacity: 128);

  void FixedUpdate()
  {
    if (!DoNavigation)
      return;
    var from = transform.position;
    var to = Target ? Target.transform.position : Vector3.zero;
    NavGraph.CalculatePath(from, to, Nodes, NavigationNeighborhoods.AsList());
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