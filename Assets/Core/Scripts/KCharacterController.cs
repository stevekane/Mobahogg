using UnityEngine;
using KinematicCharacterController;

[RequireComponent(typeof(KinematicCharacterMotor))]
[DefaultExecutionOrder((int)ExecutionGroups.State+1)]
public class KCharacterController : MonoBehaviour, ICharacterController {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] KinematicCharacterMotor Motor;
  [SerializeField] bool ShowDebug;

  public readonly FloatMinAttribute PhysicsScale = new(1);
  public readonly BooleanAnyAttribute ForceUnground = new();
  public readonly Vector3Attribute Acceleration = new();
  public readonly Vector3Attribute Velocity = new();
  public readonly Vector3Attribute DirectVelocity = new();
  public readonly QuaternionAttribute Rotation = new();

  public bool IsGrounded => Motor.GroundingStatus.FoundAnyGround;
  public bool Falling => Velocity.Current.y <= 0;
  public bool Rising => Velocity.Current.y > 0;

  void Awake() {
    Rotation.Set(Quaternion.LookRotation(transform.forward));
    Rotation.Sync();
    Motor.CharacterController = this;
  }

  void OnDestroy() {
    Motor.CharacterController = null;
  }

  public void BeforeCharacterUpdate(float deltaTime) {}

  public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
    Rotation.Sync();
    currentRotation = Rotation.Current;
  }

  public void UpdateVelocity(ref Vector3 currentVelocity, float dt) {
    // Sync unground and run if needed
    ForceUnground.Sync();
    // TODO: Should unground if frozen?
    if (ForceUnground.Current) {
      Motor.ForceUnground();
    }

    // Apply all frame modifiers
    Velocity.Sync(reset: false);
    // Add affect of accelerations
    PhysicsScale.Sync();
    Acceleration.Sync();
    Velocity.Add(PhysicsScale.Current * LocalClock.DeltaTime() * Acceleration.Current);
    // zero out y component if we're firmly on the ground
    if (IsGrounded && Falling) {
      Velocity.SetY(0);
    }
    Velocity.Sync(reset: false);

    DirectVelocity.Sync();
    currentVelocity = LocalClock.Frozen()
      ? Vector3.zero
      : Velocity.Current + DirectVelocity.Current;
  }

  public void AfterCharacterUpdate(float deltaTime) {}

  public bool IsColliderValidForCollisions(Collider coll) => true;

  public void OnDiscreteCollisionDetected(Collider hitCollider) {}

  public void OnGroundHit(
  Collider hitCollider,
  Vector3 hitNormal,
  Vector3 hitPoint,
  ref HitStabilityReport hitStabilityReport) {}

  public void OnMovementHit(
  Collider hitCollider,
  Vector3 hitNormal,
  Vector3 hitPoint,
  ref HitStabilityReport hitStabilityReport) {}

  public void PostGroundingUpdate(float deltaTime) {}

  public void ProcessHitStabilityReport(
  Collider hitCollider,
  Vector3 hitNormal,
  Vector3 hitPoint,
  Vector3 atCharacterPosition,
  Quaternion atCharacterRotation,
  ref HitStabilityReport hitStabilityReport) {}

  public void OnGUI() {
    if (!ShowDebug)
      return;
    GUILayout.BeginVertical("box");
    GUILayout.Label($"Grounded : {IsGrounded}");
    GUILayout.Label($"Force Unground : {ForceUnground.Current}");
    GUILayout.Label($"PhysicsScale : {PhysicsScale.Current}");
    GUILayout.Label($"Acceleration : {Acceleration.Current}");
    GUILayout.Label($"Velocity : {Velocity.Current}");
    GUILayout.Label($"Direct Velocity : {DirectVelocity.Current}");
    GUILayout.EndVertical();
  }
}