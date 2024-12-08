using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class GolemAttractor : MonoBehaviour {
  public Team DefeatedTeam;
  public BoxCollider ZoneCollider;
  public Collider BellCollider;

  void OnHurt(Combatant attacker) {
    GolemManager.Active.AlertGolemsTo(this);
  }

  void Start() {
    GolemManager.Active.GolemAttractors.Add(this);
  }

  void OnDestroy() {
    GolemManager.Active.GolemAttractors.Remove(this);
  }
}