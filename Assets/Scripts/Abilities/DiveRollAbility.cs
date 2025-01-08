using UnityEngine;
using Abilities;

public class DiveRollAbility : MonoBehaviour, IAbility<Vector2>, Async, Cancellable {
  [Header("Reads From")]
  [SerializeField] LocalClock LocalClock;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] int FrameDuration = 60;
  [SerializeField] float RootMotionMultiplier = 2;
  [SerializeField] float TurnSpeed = 180;
  [Header("Writes To")]
  [SerializeField] Animator Animator;
  [SerializeField] SpellAffected SpellAffected;
  [SerializeField] KCharacterController CharacterController;

  int Frame;

  void Start() {
    Frame = FrameDuration;
    AnimatorCallbackHandler.OnRootMotion.Listen(OnAnimatorMove);
  }

  void OnDestroy() {
    Frame = FrameDuration;
    AnimatorCallbackHandler.OnRootMotion.Unlisten(OnAnimatorMove);
  }

  public bool IsRunning => Frame < FrameDuration;

  public bool CanRun => true;

  public bool CanCancel => false;

  public bool CanSteer => Frame > 0 && IsRunning;

  public void Run(Vector2 input) {
    if (input.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(input.XZ().normalized));
    }
    Animator.SetTrigger("Dash");
    Frame = 0;
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

  public void Cancel() {
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