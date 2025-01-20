using UnityEngine;
using KinematicCharacterController;

[RequireComponent(typeof(KinematicCharacterMotor))]
[DefaultExecutionOrder((int)ExecutionGroups.State+1)]
public class KCharacterController : MonoBehaviour, ICharacterController {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] KinematicCharacterMotor Motor;
  [SerializeField] bool ShowDebug;

  public readonly BooleanAnyAttribute ForceUnground = new();
  // use this to control how much acceleration is applied
  public readonly Vector3Attribute AccelerationScale = Vector3Attribute.WithDefault(Vector3.one);
  public readonly Vector3Attribute VelocityScale = Vector3Attribute.WithDefault(Vector3.one);
  public readonly Vector3Attribute Acceleration = new();
  public readonly Vector3Attribute Velocity = new();
  public readonly Vector3Attribute DirectVelocity = new();
  public readonly QuaternionAttribute Rotation = new();

  public bool JustLanded { get; private set; }
  public bool JustTookOff { get; private set; }
  public bool IsGrounded { get; private set; }
  public bool Falling => Velocity.Current.y <= 0;
  public bool Rising => Velocity.Current.y > 0;

  public readonly EventSource OnLand = new();
  public readonly EventSource OnTakeOff = new();
  public int LastGroundedFrame { get; private set; }

  void Awake() {
    AccelerationScale.Set(Vector3.one);
    AccelerationScale.Sync(reset: false);
    VelocityScale.Set(Vector3.one);
    VelocityScale.Sync(reset: false);
    Rotation.Set(Quaternion.LookRotation(transform.forward));
    Rotation.Sync();
    Motor.CharacterController = this;
  }

  void OnDestroy() {
    Motor.CharacterController = null;
  }

  public void BeforeCharacterUpdate(float deltaTime) {
    ForceUnground.Sync();
    if (ForceUnground.Current) {
      Motor.ForceUnground();
    }
  }

  public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
    Rotation.Sync();
    currentRotation = Rotation.Current;
  }

  public void UpdateVelocity(ref Vector3 currentVelocity, float dt) {
    AccelerationScale.Sync(reset: false);
    Acceleration.Sync();
    Velocity.Add(LocalClock.DeltaTime() * Acceleration.Current.ComponentMultiply(AccelerationScale.Current));
    Velocity.Sync(reset: false);
    if (IsGrounded && Falling) {
      Velocity.SetY(-1);
      Velocity.Sync(reset: false);
    }
    DirectVelocity.Sync();
    currentVelocity = LocalClock.DeltaFrames() * (Velocity.Current + DirectVelocity.Current);
  }

  public void AfterCharacterUpdate(float deltaTime) {
    var wasGrounded = IsGrounded;
    IsGrounded = Motor.GroundingStatus.FoundAnyGround;
    JustTookOff = wasGrounded && !IsGrounded;
    JustLanded = !wasGrounded && IsGrounded;
    LastGroundedFrame = IsGrounded ? LocalClock.FixedFrame() : LastGroundedFrame;
    if (JustTookOff)
      OnTakeOff.Fire();
    if (JustLanded)
      OnLand.Fire();
  }

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

  void OnGUI() {
    if (!ShowDebug)
      return;
    GUILayout.BeginVertical("box");
    GUILayout.Label($"Grounded : {IsGrounded}");
    GUILayout.Label($"Force Unground : {ForceUnground.Current}");
    GUILayout.Label($"AccelerationScale : {AccelerationScale.Current}");
    GUILayout.Label($"Acceleration : {Acceleration.Current}");
    GUILayout.Label($"VelocityScale : {VelocityScale.Current}");
    GUILayout.Label($"Velocity : {Velocity.Current}");
    GUILayout.Label($"Direct Velocity : {DirectVelocity.Current}");
    GUILayout.Label($"Stable Ground: {Motor.GroundingStatus.IsStableOnGround}");
    GUILayout.EndVertical();
  }
}