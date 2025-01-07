using UnityEngine;
using State;
using Abilities;

public class TurnAbility : MonoBehaviour, IAbility<Vector2> {
  [Header("Reads From")]
  [SerializeField] TurnSpeed TurnSpeed;
  [SerializeField] LocalClock LocalClock;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  public bool CanRun => true;

  public void Run(Vector2 value) {
    if (value.magnitude > 0) {
      var desiredForward = value.XZ().normalized;
      var currentForward = CharacterController.Rotation.Forward.XZ();
      var currentRotation = Quaternion.LookRotation(currentForward);
      var desiredRotation = Quaternion.LookRotation(desiredForward);
      var turnSpeed = TurnSpeed.Value * LocalClock.DeltaTime();
      var nextForward = Quaternion.RotateTowards(currentRotation, desiredRotation, turnSpeed);
      CharacterController.Rotation.Set(nextForward);
    }
  }
}