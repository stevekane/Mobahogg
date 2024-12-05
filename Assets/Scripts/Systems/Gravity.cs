using State;
using UnityEngine;

public class Gravity : MonoBehaviour {
  [Header("Read From")]
  [SerializeField] FallSpeed FallSpeed;
  [SerializeField] LocalGravity LocalGravity;
  [Header("Write To")]
  [SerializeField] KCharacterController CharacterController;

  void FixedUpdate() {
    if (!LocalGravity.Value)
      return;
    CharacterController.PhysicsAcceleration.y += FallSpeed.Value;
  }
}