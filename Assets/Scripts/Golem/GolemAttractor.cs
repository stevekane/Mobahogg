using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class GolemAttractor : MonoBehaviour {
  public Team DefeatedTeam;
  public BoxCollider ZoneCollider;
  public Collider BellCollider;
}