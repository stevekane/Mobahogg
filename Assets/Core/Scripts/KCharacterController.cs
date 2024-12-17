using UnityEngine;
using KinematicCharacterController;
using System;

[RequireComponent(typeof(KinematicCharacterMotor))]
[DefaultExecutionOrder(-101)] // Runs right before PhysicsSystem
public class KCharacterController : MonoBehaviour, ICharacterController {
  [SerializeField] LocalClock LocalClock;


  public Vector3 Acceleration;
  public Vector3 Velocity;

  public Action<HitStabilityReport> OnCollision;
  public Action<HitStabilityReport> OnLedge;

  /*
  I think just like other stateful systems, we want to control the update order here.
  We also want to control the double-buffering of the writable values including
  Forward
  Position
  Rotation
  Velocity
  Acceleration
  UnGrounded
  */

  public KinematicCharacterMotor Motor;

  public void Unground() {
    Motor.ForceUnground();
  }

  public void Launch(Vector3 velocity) {
    Unground();
    Velocity += velocity;
    Velocity.y = velocity.y;
  }

  public Vector3 Position {
    get => transform.position;
    set => Motor.SetPosition(value);
  }

  public Vector3 Forward {
    get => transform.forward;
    set => Motor.SetRotation(Quaternion.LookRotation(value, Vector3.up));
  }

  public Quaternion Rotation {
    get => transform.rotation;
    set => Motor.SetRotation(value);
  }

  public bool DirectMove;

  public Collider GroundCollider => Motor.GroundingStatus.GroundCollider;
  public bool IsGrounded => Motor.GroundingStatus.FoundAnyGround;
  public bool IsStableOnGround => Motor.GroundingStatus.IsStableOnGround;

  void Awake() {
    Motor.CharacterController = this;
  }

  void OnDestroy() {
    Motor.CharacterController = null;
  }

  public void BeforeCharacterUpdate(float deltaTime) {
  }

  public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
  }

  public void UpdateVelocity(ref Vector3 currentVelocity, float dt) {
    var localDeltaTime = LocalClock.Frozen() ? 0 : dt;
    Velocity += localDeltaTime * Acceleration;
    Velocity.y = (IsGrounded && Velocity.y < 0) ? 0 : Velocity.y;
    Acceleration = Vector3.zero;
    currentVelocity = LocalClock.Frozen() ? Vector3.zero : Velocity;
  }

  public void AfterCharacterUpdate(float deltaTime) {
  }

  public bool IsColliderValidForCollisions(Collider coll) {
    return true;
  }

  public void OnDiscreteCollisionDetected(Collider hitCollider) {
  }

  public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
  }

  public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
    OnCollision?.Invoke(hitStabilityReport);
  }

  public void PostGroundingUpdate(float deltaTime) {
  }

  public bool IsOnLedge = false;
  public Vector3 LedgeDirection;
  public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {
    IsOnLedge = hitStabilityReport.LedgeDetected;
    LedgeDirection = hitStabilityReport.LedgeFacingDirection;
    if (IsOnLedge)
      OnLedge?.Invoke(hitStabilityReport);
  }
}