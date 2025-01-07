using State;
using UnityEngine;
using Abilities;

public class DiveRollAbility : MonoBehaviour, IAbility<Vector2>, Async, Cancellable {
  [Header("Reads From")]
  [SerializeField] LocalClock LocalClock;
  [SerializeField] TurnSpeed TurnSpeed;
  [SerializeField] Animator Animator;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] int FrameDuration = 60;
  [SerializeField] float RootMotionMultiplier = 2;
  [Header("Writes To")]
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

  public void Run(Vector2 direction) {
    Animator.SetTrigger("Dash");
    Frame = 0;
  }

  public void Cancel() {
    Frame = FrameDuration;
    Animator.SetTrigger("Cancel");
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
      TurnSpeed.Set(180);
    }
    SpellAffected.Immune.Set(IsRunning);
    Frame = Mathf.Min(FrameDuration, Frame+LocalClock.DeltaFrames());
  }
}