using State;
using UnityEngine;

public class MoveAbility : MonoBehaviour {
  [Header("Reads From")]
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] LocalClock LocalClock;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  Vector3 TruncateByMagnitude(Vector3 v, float maxMagnitude) =>
    Mathf.Min(v.magnitude, maxMagnitude) * v.normalized;

  public bool CanRun()
    => MoveSpeed.Value > 0
    && !LocalClock.Frozen();

  public bool TryRun(Vector2 value) {
    if (CanRun()) {
      var currentVelocity = CharacterController.Velocity.XZ();
      var desiredVelocity = MoveSpeed.Value * value.XZ().normalized;
      var maxMoveSpeed = MoveSpeed.Value;
      var targetVelocity = (desiredVelocity-currentVelocity).XZ();
      var steeringVelocity = TruncateByMagnitude(targetVelocity, maxMoveSpeed);
      CharacterController.Acceleration += steeringVelocity / LocalClock.DeltaTime();
      return true;
    } else {
      return false;
    }
  }
}