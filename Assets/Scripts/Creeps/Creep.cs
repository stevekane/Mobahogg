using UnityEngine;
using State;

public class Creep : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;

  public CreepManager CreepManager;

  CreepOwner LastAttacker;

  void OnHurt(Combatant attacker) {
    Health.Add(-1);
    LastAttacker = attacker.GetComponent<CreepOwner>();
    Debug.Log($"{attacker.name} dealt 1 damage to {name}");
  }

  void FixedUpdate() {
    if (!LocalClock.Frozen() && Health.Value <= 0) {
      CreepManager.OnCreepDeath(this, LastAttacker);
    }
  }

  void OnDrawGizmosSelected() {
    Debug.Log(Health.Value);
  }
}