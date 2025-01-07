using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.State)]
public class Gravity : MonoBehaviour {
  [Header("Read From")]
  [SerializeField] AbilitySettings Settings;
  [Header("Write To")]
  [SerializeField] KCharacterController CharacterController;

  public FloatMinAttribute FallingFactor { get; private set; }

  void Start() {
    FallingFactor = new(Mathf.Max(Settings.RisingGravityFactor, Settings.FallingGravityFactor));
  }

  void FixedUpdate() {
    FallingFactor.Set(Settings.GravityFactor(CharacterController.Velocity.Current));
    FallingFactor.Sync();
    var fallingAcceleration = FallingFactor.Current * Physics.gravity;
    CharacterController.Acceleration.Add(fallingAcceleration);
  }
}