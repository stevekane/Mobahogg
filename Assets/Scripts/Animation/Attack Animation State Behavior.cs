using UnityEngine;

public class AttackAnimationStateBehavior : StateMachineBehaviour  {
  public AnimationClip Clip;
  public int ActiveFrame;
  public int RecoveryFrame;

  bool ActiveFired;
  bool RecoveryFired;
  AnimatorCallbackHandler AnimatorCallbackHandler;

  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    AnimatorCallbackHandler = animator.GetComponent<AnimatorCallbackHandler>();
    AnimatorCallbackHandler.SendMessage("AnimationEvent", "Windup");
    ActiveFired = false;
    RecoveryFired = false;
  }

  public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    AnimatorCallbackHandler.SendMessage("AnimationEvent", "Ready");
    ActiveFired = false;
    RecoveryFired = false;
  }

  public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    var duration = Clip.length;
    var frameRate = Clip.frameRate;
    var frame = Mathf.RoundToInt(stateInfo.normalizedTime * duration * frameRate);
    if (frame >= ActiveFrame && !ActiveFired) {
      ActiveFired = true;
      AnimatorCallbackHandler.SendMessage("AnimationEvent", "Active");
    }
    if (frame >= RecoveryFrame && !RecoveryFired) {
      RecoveryFired = true;
      AnimatorCallbackHandler.SendMessage("AnimationEvent", "Recovery");
    }
  }
}