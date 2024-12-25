using System.Collections.Generic;
using AimAssist;
using State;
using UnityEngine;
using UnityEngine.VFX;

public class AttackAbility : MonoBehaviour, IAbility<Vector2> {
  [Header("Reads From")]
  [SerializeField] AimAssistTargeter AimAssistTargeter;
  [SerializeField] AbilitySettings Settings;
  [SerializeField] AimAssistQuery AimAssistQuery;
  [SerializeField] Player Player;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] float ForwardMotion = 1;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;
  [SerializeField] Animator Animator;
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] TurnSpeed TurnSpeed;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] VisualEffect VisualEffect;

  int Frame;
  List<Combatant> Struck = new(16);

  void Awake() {
    Frame = Settings.TotalAttackFrames;
    Hitbox.CollisionEnabled = false;
  }

  public bool IsRunning => Frame < Settings.TotalAttackFrames;
  public bool InWindup => Frame >= 0 && Frame < Settings.ActiveStartFrame;
  public bool InActive => Frame >= Settings.ActiveStartFrame && Frame < Settings.ActiveEndFrame;
  public bool InRecovery => Frame >= Settings.RecoveryStartFrame && Frame <= Settings.RecoveryEndFrame;
  public int WindupFrames => Settings.ActiveStartFrame;

  // TODO: Currently, hitbox still sends messages to involved combatants. Maybe wrong?
  // Maybe that decision-making ought to happen here?
  public bool ShouldHit(Combatant combatant) => !Struck.Contains(combatant);
  public void Hit(Combatant combatant) => Struck.Add(combatant);

  public bool CanRun
    => CharacterController.IsGrounded
    && !LocalClock.Frozen()
    && (!IsRunning || (InRecovery && Struck.Count > 0))
    && !Player.DiveRollAbility.IsRunning
    && !Player.SpellCastAbility.IsRunning;

  public bool TryRun(Vector2 direction) {
    if (CanRun) {
      // TODO: Possibly use the previously struck list to inform the aim assist system further?
      var bestTarget = AimAssistManager.Instance.BestTarget(AimAssistTargeter, AimAssistQuery);
      if (bestTarget) {
        var delta = bestTarget.transform.position-transform.position;
        CharacterController.Rotation.Set(Quaternion.LookRotation(delta.normalized));
      } else if (direction.magnitude > 0) {
        CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ()));
      }
      Struck.Clear();
      Animator.SetTrigger("Attack");
      VisualEffect.Play();
      Frame = 0;
      return true;
    } else {
      return false;
    }
  }

  public void Cancel() {
    Frame = Settings.TotalAttackFrames;
  }

  void FixedUpdate() {
    if (!IsRunning)
      return;
    Hitbox.CollisionEnabled = InActive;
    MoveSpeed.Set(0);
    TurnSpeed.Set(0);
    if (InWindup) {
      var speed = ForwardMotion / (WindupFrames * LocalClock.DeltaTime());
      CharacterController.DirectVelocity.Add(speed * CharacterController.Rotation.Forward);
    }
    Frame = Mathf.Min(Settings.TotalAttackFrames, Frame+LocalClock.DeltaFrames());
  }
}