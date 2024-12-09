using UnityEngine;

public class AttackAbility : MonoBehaviour {
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] Player Player;
  [SerializeField] AbilitySettings Settings;
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Hitbox Hitbox;

  int Frame;

  void Awake() {
    Frame = Settings.TotalAttackFrames;
    Hitbox.CollisionEnabled = false;
  }

  public bool IsRunning => Frame < Settings.TotalAttackFrames;

  public bool CanRun
    => CharacterController.IsGrounded
    && !Player.IsDashing()
    && (Frame < Settings.WindupAttackFrames || Frame >= Settings.TotalAttackFrames);

  public bool TryRun() {
    if (CanRun) {
      Animator.SetTrigger("Attack");
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
    Frame = Mathf.Min(Settings.TotalAttackFrames, Frame+LocalClock.DeltaFrames());
    Hitbox.CollisionEnabled = Frame >= Settings.ActiveStartFrame && Frame < Settings.ActiveEndFrame;
  }
}