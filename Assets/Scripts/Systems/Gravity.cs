using UnityEngine;

public class Gravity : MonoBehaviour {
  [Header("Read From")]
  [SerializeField] AbilitySettings Settings;
  [Header("Write To")]
  [SerializeField] KCharacterController CharacterController;

  void FixedUpdate() {
    var fallSpeed = Settings.GravityFactor(CharacterController.Velocity.Current) * Physics.gravity.y;
    CharacterController.Acceleration.Add(new (0, fallSpeed, 0));
  }
}