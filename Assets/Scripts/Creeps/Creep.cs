using UnityEngine;
using State;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Creep : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;

  CreepOwner LastAttacker;

  void Start() {
    CreepManager.Active.LivingCreeps.Add(this);
  }

  void OnDestroy() {
    CreepManager.Active.LivingCreeps.Remove(this);
  }

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