using State;
using UnityEngine;

public class DiveRollAbility : MonoBehaviour, IAbility<Vector2> {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Player Player;
  [SerializeField] TurnSpeed TurnSpeed;
  [SerializeField] SpellAffected SpellAffected;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] ResidualImageRenderer ResidualImageRenderer;
  [SerializeField] int FrameDuration = 60;
  [SerializeField] float RootMotionMultiplier = 2;

  int Frame;

  void Start() {
    Frame = FrameDuration;
    AnimatorCallbackHandler.OnRootMotion.Listen(OnAnimatorMove);
    AnimatorCallbackHandler.OnEvent.Listen(OnEvent);
  }

  void OnDestroy() {
    Frame = FrameDuration;
    AnimatorCallbackHandler.OnRootMotion.Unlisten(OnAnimatorMove);
    AnimatorCallbackHandler.OnEvent.Unlisten(OnEvent);
  }

  public bool IsRunning => Frame < FrameDuration;

  public bool CanRun
    => !LocalClock.Frozen()
    && !Player.AbilityActive
    && CharacterController.IsGrounded;

  public bool TryRun(Vector2 direction) {
    if (CanRun) {
      AnimatorCallbackHandler.Animator.SetTrigger("Dash");
      Frame = 0;
      return true;
    } else {
      return false;
    }
  }

  void OnAnimatorMove() {
    if (!IsRunning)
      return;
    var forward = CharacterController.Rotation.Forward;
    var dp = Vector3.Project(AnimatorCallbackHandler.Animator.deltaPosition, forward);
    var v = dp / LocalClock.DeltaTime();
    CharacterController.DirectVelocity.Add(RootMotionMultiplier * v);
  }

  void OnEvent(string name) {
    if (!IsRunning)
      return;
    if (name == "Image") {
      ResidualImageRenderer.RenderImage();
    }
  }

  void FixedUpdate() {
    if (IsRunning) {
      TurnSpeed.Set(180);
    }
    SpellAffected.Immune.Set(IsRunning);
    Frame = Mathf.Min(FrameDuration, Frame+LocalClock.DeltaFrames());
  }
}