using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class Gravity : MonoBehaviour {
  [SerializeField] AbilitySettings Settings;
  [SerializeField] KCharacterController CharacterController;

  void FixedUpdate() {
    CharacterController.Acceleration.Add(Settings.Gravity(CharacterController.Velocity.Current));
  }
}