using UnityEngine;
using KinematicCharacterController;
using System;

[RequireComponent(typeof(KinematicCharacterMotor))]
[DefaultExecutionOrder(-101)] // Runs right before PhysicsSystem
public class KCharacterController : MonoBehaviour, ICharacterController {
  public Vector3 PhysicsAcceleration;
  public Vector3 PhysicsVelocity;

  public Action<HitStabilityReport> OnCollision;
  public Action<HitStabilityReport> OnLedge;

  public KinematicCharacterMotor Motor;

  public void Unground() {
    Motor.ForceUnground();
  }

  public void Launch(Vector3 acceleration) {
    Unground();
    PhysicsVelocity.y = 0;
    PhysicsAcceleration += acceleration;
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
    this.InitComponent(out Motor);
  }

  void Start() {
    Motor.CharacterController = this;
  }

  void OnEnable() {
    Motor.enabled = true;
  }

  void OnDisable() {
    Motor.enabled = false;
  }

  void OnDestroy() {
    Motor.CharacterController = null;
  }

  public void BeforeCharacterUpdate(float deltaTime) {
  }

  public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
  }

  public void UpdateVelocity(ref Vector3 currentVelocity, float dt) {
    PhysicsVelocity += dt * PhysicsAcceleration;
    PhysicsVelocity.y = (IsGrounded && PhysicsVelocity.y < 0) ? 0 : PhysicsVelocity.y;
    currentVelocity = PhysicsVelocity;
    PhysicsAcceleration = Vector3.zero;
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