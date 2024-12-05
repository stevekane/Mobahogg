using UnityEngine;

[CreateAssetMenu(fileName = "AbilitySettings", menuName = "Scriptable Objects/AbilitySettings")]
public class AbilitySettings : ScriptableObject {
  [Header("Gravity")]
  public float RisingGravityFactor = 4;
  public float FallingGravityFactor = 8;
  public float GravityFactor(Vector3 velocity) =>
    velocity.y <= 0 ? FallingGravityFactor : RisingGravityFactor;

  [Header("Move")]
  public float GroundMoveSpeed = 5;
  public float AirSpeedDecayFactor = 0.25f;
  public float AirMoveSpeed(Vector3 velocity) =>
    GroundMoveSpeed * Mathf.Exp(-AirSpeedDecayFactor*velocity.XZ().magnitude);

  [Header("Jump")]
  float JumpHeight = 2;
  public float InitialJumpSpeed(float gravity) =>
    Mathf.Sqrt(2 * Mathf.Abs(RisingGravityFactor * gravity) * JumpHeight);

  [Header("Dash")]
  public int DashTotalFrames = 6;
  public float DashTotalDistance = 2f;
  public float DashSpeed(float secondsPerFrame) => DashTotalDistance / DashTotalFrames / secondsPerFrame;

  [Header("Attack")]
  public int WindupAttackFrames = 3;
  public int ActiveAttackFrames = 3;
  public int RecoveryAttackFrames = 6;

  [Header("Spin")]
  public int TotalSpinFrames = 30;
}