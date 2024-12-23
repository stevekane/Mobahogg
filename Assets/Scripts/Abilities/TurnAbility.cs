using State;
using UnityEngine;

public class TurnAbility : MonoBehaviour, IAbility<Vector2> {
  [Header("Reads From")]
  [SerializeField] TurnSpeed TurnSpeed;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] AttackAbility AttackAbility;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  public bool CanRun
    => !LocalClock.Frozen()
    && !AttackAbility.IsRunning;

  public bool TryRun(Vector2 value) {
    if (CanRun) {
      if (value.magnitude > 0) {
        var currentForward = CharacterController.Rotation.Forward;
        var currentRotation = Quaternion.LookRotation(currentForward);
        var desiredForward = value.XZ().normalized;
        var desiredRotation = Quaternion.LookRotation(desiredForward);
        var turnSpeed = TurnSpeed.Value * LocalClock.DeltaTime();
        var nextForward = Quaternion.RotateTowards(currentRotation, desiredRotation, turnSpeed);
        CharacterController.Rotation.Set(nextForward);
      }
      return true;
    } else {
      return false;
    }
  }
}