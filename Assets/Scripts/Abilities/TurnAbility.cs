using State;
using UnityEngine;

public class TurnAbility : MonoBehaviour {
  [Header("Reads From")]
  [SerializeField] TurnSpeed TurnSpeed;
  [SerializeField] LocalClock LocalClock;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  public bool CanRun()
    => TurnSpeed.Value > 0
    && !LocalClock.Frozen();
  public bool TryRun(Vector2 value) {
    if (CanRun()) {
      if (value.magnitude > 0) {
        var currentForward = CharacterController.Forward;
        var currentRotation = Quaternion.LookRotation(currentForward);
        var desiredForward = value.XZ().normalized;
        var desiredRotation = Quaternion.LookRotation(desiredForward);
        var turnSpeed = TurnSpeed.Value * LocalClock.DeltaTime();
        var nextForward = Quaternion.RotateTowards(currentRotation, desiredRotation, turnSpeed);
        CharacterController.Forward = nextForward * Vector3.forward;
      }
      return true;
    } else {
      return false;
    }
  }
}