using UnityEngine;
using State;

public class Creep : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;

  CreepOwner LastAttacker;

  void OnHurt(Combatant attacker) {
    Health.Change(-1);
    LastAttacker = attacker.GetComponent<CreepOwner>();
  }

  void FixedUpdate() {
    if (!LocalClock.Frozen() && Health.Value <= 0) {
      CreepManager.Active.OnCreepDeath(this, LastAttacker);
    }
  }
}