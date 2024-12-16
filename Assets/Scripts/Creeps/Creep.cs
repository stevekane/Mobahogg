using UnityEngine;
using State;
using Melee;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Creep : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;

  CreepOwner LastAttacker;

  public void OnHurt(MeleeAttackEvent attackEvent) {
    LastAttacker = attackEvent.Attacker.GetComponent<CreepOwner>();
  }

  void Start() {
    CreepManager.Active.LivingCreeps.Add(this);
  }

  void OnDestroy() {
    CreepManager.Active.LivingCreeps.Remove(this);
  }

  void FixedUpdate() {
    if (!LocalClock.Frozen() && Health.CurrentValue <= 0) {
      CreepManager.Active.OnCreepDeath(this, LastAttacker);
    }
  }
}