using UnityEngine;
using Abilities;

public class DiveRollAbility : Ability {
  [Header("Reads From")]
  [SerializeField] int FrameDuration = 60;
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

  public override bool IsRunning => Frame < FrameDuration;

  public override bool CanRun => true;

  public override bool CanStop => false;

  public bool CanSteer => Frame > 0 && IsRunning;

  public override void Run() {
    Animator.SetTrigger("Dash");
    Frame = 0;
  }

  // Available only on first frame to set your initial heading
  public void Launch(Vector2 input) {
    if (input.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(input.XZ().normalized));
    }
  }

  // Does not happen on first frame of the ability but happens on all subsequent
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

  public override void Stop() {
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