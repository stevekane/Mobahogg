using UnityEngine;
using Abilities;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class JumpAbility : Ability {
  [SerializeField] AbilitySettings Settings;

  bool InCoyoteWindow
    => (LocalClock.FixedFrame() - CharacterController.LastGroundedFrame) < Settings.CoyoteFrameCount
    && !CharacterController.IsGrounded
    && CharacterController.Falling;

  public override bool CanRun => CharacterController.IsGrounded || InCoyoteWindow;
  public override bool CanCancel => false;
  public override bool IsRunning { get; }
  public override void Run() {
    CharacterController.ForceUnground.Set(true);
    CharacterController.Velocity.SetY(Settings.InitialJumpSpeed);
  }
  public override void Cancel() {}
}