using System.Collections.Generic;
using Melee;
using UnityEngine;

public class Hitbox : MonoBehaviour {
  [SerializeField] Damage Damage;
  [SerializeField] Combatant Combatant;
  [SerializeField] Collider Collider;
  [SerializeField] MeleeAttackConfig MeleeAttackConfig;
  [SerializeField] KnockbackScale KnockbackScale;

  List<Combatant> Struck = new(16);

  public Combatant Owner => Combatant;

  public bool CollisionEnabled {
    get => Collider.enabled;
    set {
      Struck.Clear();
      Collider.enabled = value;
    }
  }

  void OnTriggerEnter(Collider c) => HandleOverlap(c);

  void OnTriggerStay(Collider c) => HandleOverlap(c);

  void HandleOverlap(Collider c) {
    if (c.TryGetComponent(out Hurtbox hurtbox) &&
        hurtbox.Owner != Owner &&
        !Struck.Contains(hurtbox.Owner)) {
      Struck.Add(hurtbox.Owner);
      var attackEvent = new MeleeAttackEvent {
        Damage = Damage.Value,
        Config = MeleeAttackConfig,
        KnockbackScale = KnockbackScale.Value,
        Attacker = Owner,
        Victim = hurtbox.Owner
      };
      Owner.Hit(attackEvent);
      hurtbox.Owner.Hurt(attackEvent);
    }
  }
}