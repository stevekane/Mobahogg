using Melee;
using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class GolemAttractor : MonoBehaviour {
  public Team DefeatedTeam;
  public BoxCollider ZoneCollider;
  public Collider BellCollider;

  public void OnHurt(MeleeAttackEvent attackEvent) {
    GolemManager.Active.AlertGolemsTo(this);
  }

  void Start() {
    GolemManager.Active.GolemAttractors.Add(this);
  }

  void OnDestroy() {
    GolemManager.Active.GolemAttractors.Remove(this);
  }
}