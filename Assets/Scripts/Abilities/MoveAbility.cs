using State;
using UnityEngine;

public class MoveAbility : MonoBehaviour, IAbility<Vector2> {
  [Header("Reads From")]
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] LocalClock LocalClock;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  public bool CanRun => true;

  public bool TryRun(Vector2 value) {
    if (CanRun) {
      CharacterController.DirectVelocity.Add(MoveSpeed.Value * value.XZ());
      return true;
    } else {
      return false;
    }
  }
}