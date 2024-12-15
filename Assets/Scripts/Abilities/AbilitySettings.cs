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
  public float AirMoveSpeed(float groundSpeed) =>
    Mathf.Exp(-AirSpeedDecayFactor*groundSpeed);

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
  public int TotalAttackFrames => WindupAttackFrames + ActiveAttackFrames + RecoveryAttackFrames;
  public int ActiveStartFrame => WindupAttackFrames;
  public int ActiveEndFrame => WindupAttackFrames + ActiveAttackFrames;

  [Header("Spin")]
  public int TotalSpinFrames = 30;
}