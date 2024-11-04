using UnityEngine;
using KinematicCharacterController;
using System;

[RequireComponent(typeof(KinematicCharacterMotor))]
[DefaultExecutionOrder(-101)] // Runs right before PhysicsSystem
public class KCharacterController : MonoBehaviour, ICharacterController {
  public float MaxMoveSpeed;
  public Vector3 PhysicsAcceleration;
  public Vector3 PhysicsVelocity;
  public Vector3 DesiredVelocity;
  public Vector3 ScriptVelocity;
  public bool HasGravity = true;

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

  void FixedUpdate() {
    //AbilityManager.SetTag(AbilityTag.Grounded, Motor.GroundingStatus.FoundAnyGround);
    //AbilityManager.SetTag(AbilityTag.Airborne, !Motor.GroundingStatus.FoundAnyGround);
  }

  void OnDestroy() {
    Motor.CharacterController = null;
  }

  public void BeforeCharacterUpdate(float deltaTime) {
  }

  public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
  }

  public void UpdateVelocity(ref Vector3 currentVelocity, float dt) {
    if (DirectMove) {
      currentVelocity = ScriptVelocity;
    } else {
      var grounded = Motor.GroundingStatus.FoundAnyGround;
      var steeringVector = (DesiredVelocity - PhysicsVelocity).XZ();
      var desiredMagnitude = steeringVector.magnitude;
      var maxSteeringMagnitude = (grounded || !HasGravity) ? 2f * MaxMoveSpeed : 0f;
      var boundedSteeringVelocity = Mathf.Min(desiredMagnitude, maxSteeringMagnitude) * steeringVector.normalized;
      // TODO: maybe move this out of here to own gravity component?
      PhysicsAcceleration += grounded || !HasGravity ? Vector3.zero : Physics.gravity;
      PhysicsVelocity += boundedSteeringVelocity;
      PhysicsVelocity += ScriptVelocity;
      PhysicsVelocity += dt * PhysicsAcceleration;
      PhysicsVelocity.y = grounded ? 0 : PhysicsVelocity.y;
      currentVelocity = PhysicsVelocity;
    }
    const float MAX_MOVE_SPEED = 5; // TODO: This is obviously bullshit
    if (TryGetComponent(out Animator animator))
      animator.SetFloat("Normalized Move Speed", DesiredVelocity.sqrMagnitude / MAX_MOVE_SPEED);
    PhysicsAcceleration = Vector3.zero;
    DesiredVelocity = Vector3.zero;
    ScriptVelocity = Vector3.zero;
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