using UnityEngine;

public class Mover : MonoBehaviour {
  public static Quaternion RotationFromDesired(Quaternion rotation, float degreesDelta, Vector3 desiredForward) {
    var desiredRotation = Quaternion.LookRotation(desiredForward);
    return Quaternion.RotateTowards(rotation, desiredRotation, degreesDelta);
  }

  CharacterController Controller;
  Attributes Attributes;
  Status Status;
  Vector3? TeleportDestination;
  Vector3 MoveDelta;
  Quaternion TurnDelta;
  Vector3 DesiredMoveDir;
  Vector3 DesiredFacing;

  public Vector3 InputVelocity { get; private set; }
  public Vector3 FallVelocity => new(0, -FallSpeed, 0);
  public Vector3 MoveVelocity { get; private set; }
  public Vector3 Velocity { get; private set; }
  public float FallSpeed { get; private set; }
  public float WalkSpeed => Attributes.GetValue(AttributeTag.MoveSpeed) * Attributes.GetValue(AttributeTag.LocalTimeScale);

  public void Awake() {
    this.InitComponent(out Controller);
    this.InitComponent(out Attributes);
    this.InitComponent(out Status);
  }

  public void TryLookAt(Transform target) {
    if (target) {
      SetDesiredFacing((target.position-transform.position).normalized);
    }
  }

  public void SetDesiredMove(Vector3 v) => DesiredMoveDir = v;
  public void SetDesiredFacing(Vector3 v) => DesiredFacing = v;
  public void Teleport(Vector3 destination) => TeleportDestination = destination;
  public void Move(Vector3 delta) => MoveDelta += delta;
  public void Turn(Quaternion delta) => TurnDelta *= delta;
  public void ResetVelocity() {
    InputVelocity = Vector3.zero;
    FallSpeed = 0;
    MoveVelocity = Vector3.zero;
    Velocity = Vector3.zero;
  }
  public void ResetVelocityAndMovementEffects() {
    ResetVelocity();
    Status.Remove(Status.Get<KnockbackEffect>());
  }

  public bool IsOverGround(Vector3 delta) {
    const float GROUND_DISTANCE = .2f;
    var cylinderHeight = Mathf.Max(0, Controller.height - 2*Controller.radius);
    var offsetDistance = cylinderHeight / 2;
    var offset = offsetDistance*Vector3.up;
    var skinOffset = Controller.skinWidth*Vector3.up;
    var position = transform.TransformPoint(Controller.center + skinOffset - offset) + delta;
    var ray = new Ray(position, Vector3.down);
    RaycastHit hit;
    var didHit = Physics.SphereCast(ray, Controller.radius, out hit, GROUND_DISTANCE, Defaults.Instance.EnvironmentLayerMask, QueryTriggerInteraction.Ignore);
    return didHit && hit.collider.CompareTag(Defaults.Instance.GroundTag);
  }

  public void FixedUpdate() {
    var localTimeScale = Attributes.GetValue(AttributeTag.LocalTimeScale);
    var dt = localTimeScale * Time.fixedDeltaTime;

    // Move
    var moveSpeed = Attributes.GetValue(AttributeTag.MoveSpeed);
    var gravity = dt * Attributes.GetValue(AttributeTag.Gravity);
    InputVelocity = moveSpeed * DesiredMoveDir;
    FallSpeed = Status switch {
      Status { HasGravity: true, IsGrounded: true } => gravity,
      Status { HasGravity: true, IsGrounded: false } => FallSpeed + gravity,
      _ => 0
    };
    var maxFallSpeed = Attributes.GetValue(AttributeTag.MaxFallSpeed);
    FallSpeed = Mathf.Min(FallSpeed, maxFallSpeed);
    MoveVelocity = MoveDelta / Time.fixedDeltaTime;
    Velocity = InputVelocity + FallVelocity + MoveVelocity;
    var inputDelta = dt * InputVelocity;
    var fallDelta = dt * FallVelocity;
    Controller.Move(inputDelta + fallDelta + MoveDelta);
    MoveDelta = Vector3.zero;

    if (TeleportDestination.HasValue) {
      transform.position = TeleportDestination.Value;
      ResetVelocity();
      TeleportDestination = null;
    }

    // Turn
    var degrees = dt * Attributes.GetValue(AttributeTag.TurnSpeed);
    var desiredRotation = RotationFromDesired(transform.rotation, degrees, DesiredFacing.TryGetDirection() ?? transform.forward);
    transform.rotation = desiredRotation * TurnDelta;
    TurnDelta = Quaternion.identity;

    // Animation
    //var animator = AnimationDriver.Animator;
    //var orientedVelocity = Quaternion.Inverse(transform.rotation)*Velocity.XZ().normalized;
    //var inputSpeed = InputVelocity.magnitude;
    //const float MOVE_CYCLE_DISTANCE = 5; // distance moved by the walk cycle at full speed... very bullshit
    //animator.SetFloat("TorsoRotation", AnimationDriver.TorsoRotation);
    //animator.SetFloat("RightVelocity", orientedVelocity.x);
    //animator.SetFloat("ForwardVelocity", orientedVelocity.z);
    //animator.SetFloat("Speed", inputSpeed / MOVE_CYCLE_DISTANCE);
    //animator.SetBool("IsGrounded", Status.IsGrounded);
    //animator.SetBool("IsWallSliding", Status.IsWallSliding);
    //animator.SetBool("IsHurt", Status.IsHurt);
    //animator.SetBool("IsFallen", Status.IsFallen);
    //AnimationDriver.SetSpeed(localTimeScale < 1 ? localTimeScale : AnimationDriver.BaseSpeed);
  }
}