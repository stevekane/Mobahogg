using System.Collections.Generic;
using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class CannonManager : SingletonBehavior<CannonManager> {
  public float MinRange = 2;
  public float MaxRange = 30;
  public List<CannonTarget> Targets;

  public CannonTarget BestTarget(Cannon cannon) {
    var cannonTeam = cannon.GetComponent<Team>();
    CannonTarget bestTarget = null;
    float bestDistance = float.MaxValue;
    foreach (var target in Targets) {
      var targetTeam = target.GetComponent<Team>();
      var distance = Vector3.Distance(target.transform.position.XZ(), cannon.transform.position.XZ());
      var validTeam = targetTeam.TeamType != cannonTeam.TeamType;
      var validDistance = distance >= MinRange && distance <= MaxRange;
      var lowerDistance = distance < bestDistance;
      if (validTeam && validDistance && lowerDistance) {
        bestDistance = distance;
        bestTarget = target;
      }
    }
    return bestTarget;
  }
}