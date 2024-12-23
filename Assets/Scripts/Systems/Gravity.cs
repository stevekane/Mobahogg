using State;
using UnityEngine;

public class Gravity : MonoBehaviour {
  [Header("Read From")]
  [SerializeField] AbilitySettings Settings;
  [SerializeField] LocalGravity LocalGravity;
  [Header("Write To")]
  [SerializeField] KCharacterController CharacterController;

  void FixedUpdate() {
    if (!LocalGravity.Value)
      return;
    var fallSpeed = Settings.GravityFactor(CharacterController.Velocity.Current) * Physics.gravity.y;
    CharacterController.Acceleration.Add(new (0, fallSpeed, 0));
  }
}