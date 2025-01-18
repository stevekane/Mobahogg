using Abilities;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class HoverAbility : Ability {
  public bool Hovering;

  void Start() {
    CharacterController.OnLand.Listen(Cancel);
  }

  void OnDestroy() {
    CharacterController.OnLand.Unlisten(Cancel);
  }

  public override bool IsRunning => Hovering;
  public override bool CanRun => !CharacterController.IsGrounded && CharacterController.Falling;
  public override void Run() {
    Hovering = true;
  }
  public override bool CanCancel => true;
  public override void Cancel() {
    Debug.Log("Cancel presumably from landing");
    Hovering = false;
  }
}