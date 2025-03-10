using System;
using System.ComponentModel;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public partial class RootMotionBehavior : FrameBehavior {
  Animator Animator;
  Vector3 PreviousPosition;

  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, null, out Animator);
  }

  public override void PreviewCleanup(object provider) {
    Animator.applyRootMotion = false;
    Animator = null;
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    PreviousPosition = Animator.transform.position;
    Animator.applyRootMotion = true;
  }

  public override void PreviewOnEnd(PreviewRenderUtility preview) {
    Animator.applyRootMotion = false;
  }

  public override void PreviewOnLateUpdate(PreviewRenderUtility preview) {
    var currentPosition = Animator.transform.position;
    var deltaPosition = currentPosition - PreviousPosition;
    var newPosition = PreviousPosition + PositionMultiplier * deltaPosition;
    Animator.transform.position = newPosition;
    PreviousPosition = newPosition;
  }
}
#endif

[Serializable]
[DisplayName("Root Motion")]
public partial class RootMotionBehavior : FrameBehavior {
  public float PositionMultiplier = 1;

  AnimatorCallbackHandler AnimatorCallbackHandler;
  KCharacterController CharacterController;
  LocalClock LocalClock;

  void OnRootMotion() {
    var forward = CharacterController.Rotation.Forward;
    var dp = Vector3.Project(AnimatorCallbackHandler.Animator.deltaPosition, forward);
    var v = dp / LocalClock.DeltaTime();
    CharacterController.DirectVelocity.Add(PositionMultiplier * v);
  }

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out AnimatorCallbackHandler);
    TryGetValue(provider, null, out CharacterController);
    TryGetValue(provider, null, out LocalClock);
  }

  public override void OnStart() {
    AnimatorCallbackHandler.OnRootMotion.Listen(OnRootMotion);
  }

  public override void OnEnd() {
    AnimatorCallbackHandler.OnRootMotion.Unlisten(OnRootMotion);
  }
}

public class RootMotionFrameBehaviorRuntime : IFrameBehaviorInstance {
  public void Initialize() {}
  public void OnStart() {}
  public void OnUpdate() {}
  public void OnLateUpdate() {}
  public void OnEnd() {}
  public void Cleanup() {}
}

public class RootMotionFrameBehaviorPreview : IFrameBehaviorInstance {
  public void Initialize() {}
  public void OnStart() {}
  public void OnUpdate() {}
  public void OnLateUpdate() {}
  public void OnEnd() {}
  public void Cleanup() {}
}
