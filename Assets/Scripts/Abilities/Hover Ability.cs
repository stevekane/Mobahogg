using Abilities;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class HoverAbility : Ability {
  [SerializeField] AbilitySettings Settings;
  [SerializeField] SpellStaff SpellStaff;
  [SerializeField] WeaponAim WeaponAim;

  bool Hovering;

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
    WeaponAim.AimDirection = Vector3.up;
    SpellStaff.Open();
    CharacterController.AccelerationScale.SetY(0);
    CharacterController.Velocity.SetY(0);
  }
  public override bool CanCancel => true;
  public override void Cancel() {
    Hovering = false;
    WeaponAim.AimDirection = null;
    SpellStaff.Close();
    CharacterController.AccelerationScale.SetY(1);
  }

  void FixedUpdate() {
    if (IsRunning) {
      CharacterController.DirectVelocity.SetY(-Mathf.Abs(Settings.HoverVelocity));
    }
  }
}