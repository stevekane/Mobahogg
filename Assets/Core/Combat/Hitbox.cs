using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using Melee;
using UnityEngine;

public class Hitbox : MonoBehaviour {
  [SerializeField] Damage Damage;
  [SerializeField] Combatant Combatant;
  [SerializeField] Collider Collider;
  [SerializeField] MeleeAttackConfig MeleeAttackConfig;
  [SerializeField] AttackAbility AttackAbility;

  public Combatant Owner => Combatant;

  public bool CollisionEnabled {
    get => Collider.enabled;
    set => Collider.enabled = value;
  }

  void OnTriggerEnter(Collider c) => HandleOverlap(c);

  void OnTriggerStay(Collider c) => HandleOverlap(c);

  void HandleOverlap(Collider c) {
    if (c.TryGetComponent(out Hurtbox hurtbox) &&
        hurtbox.Owner != Owner &&
        AttackAbility.ShouldHit(hurtbox.Owner)) {
      AttackAbility.Hit(hurtbox.Owner);
      var attackEvent = new MeleeAttackEvent {
        Damage = Damage.Value,
        Config = MeleeAttackConfig,
        Attacker = Owner,
        Victim = hurtbox.Owner
      };
      Owner.Hit(attackEvent);
      hurtbox.Owner.Hurt(attackEvent);
    }
  }
}