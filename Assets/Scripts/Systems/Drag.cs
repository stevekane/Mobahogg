using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class Drag : MonoBehaviour {
  [Header("Reads From")]
  [SerializeField] LocalClock LocalClock;
  [SerializeField, Range(0, 1)] float DragScale = 0.5f;
  [SerializeField] float MinSpeedToStop = 0.15f;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  void FixedUpdate() {
    if (LocalClock.Frozen())
      return;
    var currentSpeed = CharacterController.Velocity.magnitude;
    var dragStrength = currentSpeed > MinSpeedToStop
      // Drag force
      ? DragScale * currentSpeed
      // Force to come to a stop
      : currentSpeed / LocalClock.DeltaTime();
    CharacterController.Acceleration += dragStrength * -CharacterController.Velocity.normalized;
  }
}