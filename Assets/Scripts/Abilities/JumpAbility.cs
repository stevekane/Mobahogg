using UnityEngine;
using Abilities;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class JumpAbility : Ability {
  [SerializeField] AbilitySettings Settings;

  // This chould possibly be moved outside this ability to a peer-start condition
  bool InCoyoteWindow
    => (LocalClock.FixedFrame() - CharacterController.LastGroundedFrame) < Settings.CoyoteFrameCount
    && !CharacterController.IsGrounded
    && CharacterController.Falling;

  public override bool CanRun => CharacterController.IsGrounded || InCoyoteWindow;
  public override bool CanCancel => false;
  public override bool IsRunning { get; }
  public override void Run() {}
  public override void Cancel() {}
}