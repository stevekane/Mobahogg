using UnityEngine;

[CreateAssetMenu(fileName = "AbilitySettings", menuName = "Scriptable Objects/AbilitySettings")]
public class AbilitySettings : ScriptableObject {
  [Header("Gravity")]
  public float RisingGravityFactor = 4;
  public float FallingGravityFactor = 8;
  public Vector3 Gravity(Vector3 velocity) =>
    (velocity.y <= 0 ? FallingGravityFactor : RisingGravityFactor) * Physics.gravity;

  [Header("Hover")]
  public float HoverVelocity = -1;

  [Header("Move")]
  public float GroundMoveSpeed = 5;

  [Header("Jump")]
  public int CoyoteFrameCount = 6;
  public float JumpHeight = 2;
  public float InitialJumpSpeed =>
    Mathf.Sqrt(2 * Mathf.Abs(RisingGravityFactor * Physics.gravity.y) * JumpHeight);
}