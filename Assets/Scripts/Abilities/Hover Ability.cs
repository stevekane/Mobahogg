using Abilities;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class HoverAbility : Ability {
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
    WeaponAim.AimDirection = Vector3.up;
    SpellStaff.Open();
    Hovering = true;
  }
  public override bool CanCancel => true;
  public override void Cancel() {
    WeaponAim.AimDirection = null;
    SpellStaff.Close();
    Hovering = false;
  }
}