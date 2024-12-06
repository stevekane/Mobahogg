using UnityEngine;
using State;

public class Creep : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;

  public CreepManager CreepManager;

  CreepOwner LastAttacker;

  void OnHurt(Combatant attacker) {
    // TODO: Technically, many blows could land here.
    // maybe need to handle this case with some kind
    // of incoming damage buffer or something?
    Health.Change(-1);
    LastAttacker = attacker.GetComponent<CreepOwner>();
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