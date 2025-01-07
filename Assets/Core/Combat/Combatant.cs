using Melee;
using State;
using UnityEngine;
using UnityEngine.Events;

public class Combatant : MonoBehaviour {
  [SerializeField] string HurtFlinchName = "Hurt Flinch";
  [SerializeField] Animator Animator;
  [SerializeField] Health Health;
  [SerializeField] HitStop HitStop;
  [SerializeField] Knockback Knockback;
  [SerializeField] Flash Flash;
  [SerializeField] Vibrator Vibrator;
  [SerializeField] UnityEvent<MeleeAttackEvent> OnHurt;
  [SerializeField] UnityEvent<MeleeAttackEvent> OnHit;

  public Combatant LastAttacker { get; private set; }

  public void Hit(MeleeAttackEvent meleeAttackEvent) {
    if (HitStop)
      HitStop.FramesRemaining = meleeAttackEvent.Config.HitStopDuration.Ticks;
    CameraManager.Instance.Shake(meleeAttackEvent.Config.CameraShakeIntensity);
    OnHit?.Invoke(meleeAttackEvent);
  }

  public void Hurt(MeleeAttackEvent meleeAttackEvent) {
    LastAttacker = meleeAttackEvent.Attacker;
    if (Animator && HurtFlinchName != "")
      Animator.SetTrigger(HurtFlinchName);
    if (Health)
      Health.Change(-meleeAttackEvent.Damage);
    if (HitStop)
      HitStop.FramesRemaining = meleeAttackEvent.Config.HitStopDuration.Ticks;
    if (Knockback)
      Knockback.Set(meleeAttackEvent.Knockback);
    if (Flash)
      Flash.Set(meleeAttackEvent.Config.HitStopDuration.Ticks);
    if (Vibrator)
      Vibrator.Vibrate(
        meleeAttackEvent.ToVictim,
        meleeAttackEvent.Config.HitStopDuration.Ticks,
        meleeAttackEvent.Config.VibrationAmplitude,
        meleeAttackEvent.Config.VibrationFrequency);
    OnHurt?.Invoke(meleeAttackEvent);
  }
}