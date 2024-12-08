using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class GolemManager : MonoBehaviour {
  public static GolemManager Active;

  public List<Golem> Golems = new();
  public List<GolemAttractor> GolemAttractors = new();

  public void AlertGolemsTo(GolemAttractor golemAttractor) {
    Golems.ForEach(g => g.PursueAttractor(golemAttractor));
  }

  void Awake() {
    Active = this;
  }

  void OnDrawGizmos() {
    foreach (var golem in Golems) {
      if (golem.IsAwake) {
        Gizmos.color = Color.red;
        var p1 = golem.transform.position+4*Vector3.up;
        var p2 = golem.CurrentAttractor.transform.position+4*Vector3.up;
        Gizmos.DrawLine(p1, p2);
      } else {
        Gizmos.color = Color.white;
        Gizmos.DrawIcon(golem.transform.position+4*Vector3.up, "Sleep.png", true);
      }
    }
    foreach (var attractor in GolemAttractors) {
      var color = attractor.DefeatedTeam.TeamType == TeamType.Turtles ? Color.red : Color.green;
      color.a = Golems.Any(g => g.CurrentAttractor == attractor) ? 0.75f : 0.25f;
      Gizmos.color = color;
      var position = attractor.ZoneCollider.transform.TransformPoint(attractor.ZoneCollider.center);
      Gizmos.DrawCube(position, attractor.ZoneCollider.size);
    }
  }
}