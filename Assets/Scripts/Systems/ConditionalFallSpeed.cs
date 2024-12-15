using UnityEngine;
using State;

public class ConditionalFallSpeed : MonoBehaviour {
  [Header("Read From")]
  [SerializeField] AbilitySettings Settings;
  [SerializeField] KCharacterController CharacterController;
  [Header("Write To")]
  [SerializeField] FallSpeed FallSpeed;

  void FixedUpdate() {
    FallSpeed.Set(Settings.GravityFactor(CharacterController.Velocity) * Physics.gravity.y);
  }
}