using Melee;
using State;
using UnityEngine;
using UnityEngine.Events;

public class Combatant : MonoBehaviour {
  [SerializeField] string HitFlinchName = "Hit Flinch";
  [SerializeField] string HurtFlinchName = "Hurt Flinch";
  [SerializeField] Health Health;
  [SerializeField] HitStop HitStop;
  [SerializeField] Knockback Knockback;
  [SerializeField] Animator Animator;
  [SerializeField] UnityEvent<MeleeAttackEvent> OnHurt;
  [SerializeField] UnityEvent<MeleeAttackEvent> OnHit;

  public void Hit(MeleeAttackEvent meleeAttackEvent) {
    if (HitStop)
      HitStop.FramesRemaining = meleeAttackEvent.Config.HitStopDuration.Ticks;
    // TODO: Have some kind of attacker knockback?
    // if (Knockback)
    //  Knockback.Add(meleeAttackEvent.KnockbackDirection, meleeAttackEvent.KnockbackFrames);
    if (Animator && HitFlinchName != "")
      Animator.SetTrigger(HitFlinchName);
    OnHit?.Invoke(meleeAttackEvent);
  }

  public void Hurt(MeleeAttackEvent meleeAttackEvent) {
    if (Health)
      Health.Change(meleeAttackEvent.Damage);
    if (HitStop)
      HitStop.FramesRemaining = meleeAttackEvent.Config.HitStopDuration.Ticks;
    if (Knockback)
      Knockback.Add(meleeAttackEvent.KnockbackDirection, meleeAttackEvent.KnockbackFrames);
    if (Animator && HurtFlinchName != "")
      Animator.SetTrigger(HurtFlinchName);
    OnHurt?.Invoke(meleeAttackEvent);
  }
}