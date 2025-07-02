using Melee;
using State;
using UnityEngine;
using UnityEngine.Events;

// Expressed from victom's POV
public enum AttackDirection {
  Front = 0,
  Right,
  Back,
  Left
}

public class Combatant : MonoBehaviour {
  [Header("Victim modifiers to normal Attacker reactions")]
  [Tooltip("Use these to affect how an attacker reacts when striking this Victim")]
  public int HitStopMultiplier = 1;

  [SerializeField] string HurtFlinchName = "Hurt Flinch";
  [SerializeField] string HurtDirectionName = "Hurt Direction";
  [SerializeField] Animator Animator;
  [SerializeField] Health Health;
  [SerializeField] HitStop HitStop;
  [SerializeField] Knockback Knockback;
  [SerializeField] Flash Flash;
  [SerializeField] Vibrator Vibrator;
  [SerializeField] UnityEvent<MeleeAttackEvent> OnHurt;
  [SerializeField] UnityEvent<MeleeAttackEvent> OnHit;

  public float ToInterpolant(AttackDirection direction) => (int)direction / (float)3;

  public AttackDirection AttackedFrom(Vector3 attackerForward, Vector3 victimForward) {
    float angle = Vector3.SignedAngle(-victimForward, attackerForward, Vector3.up);
    return angle switch {
        >= -45f and <= 45f => AttackDirection.Front,
        > 45f and <= 135f => AttackDirection.Right,
        < -45f and >= -135f => AttackDirection.Left,
        _ => AttackDirection.Back
    };
  }

  public Combatant LastAttacker { get; private set; }

  public void Hit(MeleeAttackEvent meleeAttackEvent) {
    if (HitStop)
    {
      HitStop.FramesRemaining = meleeAttackEvent.Victim.HitStopMultiplier * meleeAttackEvent.Config.HitStopDuration.Ticks;
    }
    CameraManager.Instance.Shake(meleeAttackEvent.Config.CameraShakeIntensity);
    OnHit?.Invoke(meleeAttackEvent);
  }

  public void Hurt(MeleeAttackEvent meleeAttackEvent) {
    LastAttacker = meleeAttackEvent.Attacker;
    if (Animator && HurtFlinchName != "") {
      var direction = AttackedFrom(meleeAttackEvent.Attacker.transform.forward, meleeAttackEvent.Victim.transform.forward);
      var interpolant = ToInterpolant(direction);
      Animator.SetFloat(HurtDirectionName, interpolant);
      Animator.SetTrigger(HurtFlinchName);
    }
    if (Health)
      Health.Change(-meleeAttackEvent.Damage);
    if (HitStop)
      HitStop.FramesRemaining = meleeAttackEvent.Config.HitStopDuration.Ticks;
    if (Knockback)
      Knockback.Set(meleeAttackEvent.Knockback);
    if (Flash)
      Flash.Set(meleeAttackEvent.Config.HitStopDuration.Ticks);
    if (Vibrator)
      Vibrator.StartVibrate(
        meleeAttackEvent.ToVictim,
        meleeAttackEvent.Config.HitStopDuration.Ticks,
        meleeAttackEvent.Config.VibrationAmplitude,
        meleeAttackEvent.Config.VibrationFrequency);
    OnHurt?.Invoke(meleeAttackEvent);
  }
}