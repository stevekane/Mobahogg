using System.Collections.Generic;
using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class CannonManager : SingletonBehavior<CannonManager> {
  public List<CannonTarget> Targets;

  public CannonTarget BestTarget(Cannon cannon) {
    var cannonTeam = cannon.GetComponent<Team>();
    CannonTarget bestTarget = null;
    float bestDistance = float.MaxValue;
    foreach (var target in Targets) {
      var targetTeam = target.GetComponent<Team>();
      var toTarget = target.transform.position - cannon.transform.position;
      var distance = toTarget.XZ().magnitude;
      var validTeam = !cannonTeam || cannonTeam.TeamType != targetTeam.TeamType;
      var validDistance = distance >= cannon.MinRange && distance <= cannon.MaxRange;
      var validAngle = Vector3.Angle(cannon.transform.forward, toTarget.XZ()) <= cannon.FieldOfView/2;
      var lowerDistance = distance < bestDistance;
      if (validTeam && validDistance && validAngle && lowerDistance) {
        bestDistance = distance;
        bestTarget = target;
      }
    }
    return bestTarget;
  }
}