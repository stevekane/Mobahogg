using UnityEngine;
using Abilities;

public class JumpAbility : MonoBehaviour, IAbility {
  [Header("Reads From")]
  [SerializeField] AbilitySettings Settings;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  [field:SerializeField] public bool IsJumping { get; private set; } = false;

  void Awake() {
    CharacterController.OnLand.Listen(OnLand);
  }

  void OnDestroy() {
    CharacterController.OnLand.Unlisten(OnLand);
  }

  void OnLand() {
    IsJumping = false;
  }

  public bool CanRun => true;

  public void Run() {
    var jumpSpeed = Settings.InitialJumpSpeed(Physics.gravity.y);
    CharacterController.ForceUnground.Set(true);
    CharacterController.Velocity.SetY(jumpSpeed);
    IsJumping = true;
  }
}