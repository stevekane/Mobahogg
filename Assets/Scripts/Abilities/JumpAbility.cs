using UnityEngine;

public class JumpAbility : MonoBehaviour, IAbility {
  [Header("Reads From")]
  [SerializeField] AbilitySettings Settings;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Player Player;

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

  public bool CanRun
    => !LocalClock.Frozen()
    && Player.Grounded
    && !Player.AbilityActive;

  public bool TryRun() {
    if (CanRun) {
      var jumpSpeed = Settings.InitialJumpSpeed(Physics.gravity.y);
      CharacterController.ForceUnground.Set(true);
      CharacterController.Velocity.SetY(jumpSpeed);
      IsJumping = true;
      return true;
    } else {
      IsJumping = false;
      return false;
    }
  }
}