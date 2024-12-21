using AimAssist;
using State;
using UnityEngine;
using UnityEngine.VFX;

public class AttackAbility : MonoBehaviour, IAbility {
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

  void Awake() {
    Frame = Settings.TotalAttackFrames;
    Hitbox.CollisionEnabled = false;
  }

  public bool IsRunning => Frame < Settings.TotalAttackFrames;
  public bool IsWindup => Frame >= 0 && Frame < Settings.ActiveStartFrame;
  public bool IsActive => Frame >= Settings.ActiveStartFrame && Frame < Settings.ActiveEndFrame;
  public int WindupFrames => Settings.ActiveStartFrame;

  public bool CanRun
    => CharacterController.IsGrounded
    && !Player.IsDashing()
    && (Frame < Settings.WindupAttackFrames || Frame >= Settings.TotalAttackFrames)
    && !LocalClock.Frozen();

  public bool TryRun() {
    Debug.Log($"Attack TryRun {TimeManager.Instance.FixedFrame()}");
    if (CanRun) {
      var bestTarget = AimAssistManager.Instance.BestTarget(AimAssistTargeter, AimAssistQuery);
      if (bestTarget) {
        var delta = bestTarget.transform.position-transform.position;
        var direction = delta.normalized;
        CharacterController.Forward = direction;
      }
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
    Hitbox.CollisionEnabled = IsActive;
    MoveSpeed.Set(0);
    TurnSpeed.Set(0);
    if (IsWindup) {
      var speed = ForwardMotion / (WindupFrames * Time.fixedDeltaTime);
      CharacterController.Velocity = speed * CharacterController.Forward;
    } else {
      CharacterController.Velocity = Vector3.zero;
    }
    Frame = Mathf.Min(Settings.TotalAttackFrames, Frame+LocalClock.DeltaFrames());
  }
}