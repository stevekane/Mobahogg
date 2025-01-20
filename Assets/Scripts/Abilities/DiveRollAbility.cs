using UnityEngine;
using Abilities;

public class DiveRollAbility : Ability {
  [Header("Reads From")]
  [SerializeField] int FrameDuration = 60;
  [SerializeField] int CancelFrames = 10;
  [SerializeField] float RootMotionMultiplier = 2;
  [SerializeField] float TurnSpeed = 180;
  [Header("Writes To")]
  [SerializeField] SpellAffected SpellAffected;

  int Frame;

  void Start() {
    Frame = FrameDuration;
    AnimatorCallbackHandler.OnRootMotion.Listen(OnAnimatorMove);
  }

  void OnDestroy() {
    Frame = FrameDuration;
    AnimatorCallbackHandler.OnRootMotion.Unlisten(OnAnimatorMove);
  }

  // TODO: Could include Groundedness here as an internal requirement?
  public override bool IsRunning => Frame < FrameDuration;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    Animator.SetTrigger("Dash");
    Frame = 0;
  }

  // Available only on first frame to set your initial heading
  public bool CanLaunch => IsRunning && Frame == 0;
  public void Launch(Vector2 input) {
    if (input.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(input.XZ().normalized));
    }
  }

  public bool CanSteer => IsRunning && Frame > 0;
  public void Steer(Vector2 input) {
    if (input.magnitude > 0) {
      var desiredForward = input.XZ().normalized;
      var currentForward = CharacterController.Rotation.Forward.XZ();
      var currentRotation = Quaternion.LookRotation(currentForward);
      var desiredRotation = Quaternion.LookRotation(desiredForward);
      var maxDegrees = TurnSpeed * LocalClock.DeltaTime();
      var nextForward = Quaternion.RotateTowards(currentRotation, desiredRotation, maxDegrees);
      CharacterController.Rotation.Set(nextForward);
    }
  }

  public override bool CanCancel => IsRunning && Frame > (FrameDuration-CancelFrames);
  public override void Cancel() {
    Animator.SetTrigger("Cancel");
    Frame = FrameDuration;
  }

  void OnAnimatorMove() {
    if (!IsRunning)
      return;
    var forward = CharacterController.Rotation.Forward;
    var dp = Vector3.Project(AnimatorCallbackHandler.Animator.deltaPosition, forward);
    var v = dp / LocalClock.DeltaTime();
    CharacterController.DirectVelocity.Add(RootMotionMultiplier * v);
  }

  void FixedUpdate() {
    if (IsRunning) {
      SpellAffected.Immune.Set(true);
      Frame = Mathf.Min(FrameDuration, Frame+LocalClock.DeltaFrames());
    }
  }
}