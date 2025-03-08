using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.ComponentModel;

#if UNITY_EDITOR
using UnityEditor;

public partial class AnimationOneShotFrameBehavior : IInstanceablePreview {
  public IFrameBehaviorInstance CreatePreviewInstance(object provider) {
    return new AnimationOneShotFrameBehaviorPreview() {
      Behavior = this,
      Animator = TryGet<Animator>(provider, null)
    };
  }
}

public partial class AnimationOneShotFrameBehavior {
  PlayableGraph PlayableGraph;
  AnimatorControllerPlayable AnimatorControllerPlayable;

  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, null, out Animator);
    PlayableGraph = PlayableGraph.Create("Animation One Shot Preview");
    PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    AnimatorControllerPlayable = AnimatorControllerPlayable.Create(PlayableGraph, Animator.runtimeAnimatorController);
    var output = AnimationPlayableOutput.Create(PlayableGraph, "Animation Output", Animator);
    output.SetSourcePlayable(AnimatorControllerPlayable);
  }

  public override void PreviewCleanup(object provider) {
    if (PlayableGraph.IsValid()) {
      PlayableGraph.Destroy();
    }
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    AnimatorControllerPlayable.CrossFadeInFixedTime(StartStateName, CrossFadeDuration, LayerIndex);
  }

  public override void PreviewOnUpdate(PreviewRenderUtility preview) {
    PlayableGraph.Evaluate(Time.fixedDeltaTime);
  }
}
#endif

[Serializable]
[DisplayName("Animation One Shot")]
public partial class AnimationOneShotFrameBehavior : FrameBehavior {
  const string OnEndStateName = "Layer Open";

  public AnimationClip AnimationClip;
  public string StartStateName;
  public int LayerIndex;
  public float CrossFadeDuration = 0.1f;

  Animator Animator;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Animator);
  }

  public override void OnStart() {
    Animator.CrossFadeInFixedTime(
      StartStateName,
      CrossFadeDuration,
      LayerIndex);
  }

  public override void OnEnd() {
    Animator.Play(OnEndStateName, LayerIndex);
  }
}

public interface IInstanceableRuntime {
  public IFrameBehaviorInstance CreateRuntimeInstance(object provider);
}

public interface IInstanceablePreview {
  public IFrameBehaviorInstance CreatePreviewInstance(object provider);
}

public interface IFrameBehaviorInstance {
  public void Initialize();
  public void OnStart();
  public void OnUpdate();
  public void OnLateUpdate();
  public void OnEnd();
  public void Cleanup();
}

public interface ISeekable {
  public void Seek(int frame);
}

public class AnimationOneShotFrameBehaviorPreview : IFrameBehaviorInstance {
  public AnimationOneShotFrameBehavior Behavior;
  public Animator Animator;
  public void Initialize() {}
  public void OnStart() {}
  public void OnUpdate() {}
  public void OnLateUpdate() {}
  public void OnEnd() {}
  public void Cleanup() {}
}

public class AnimationOneShotFrameBehaviorRuntime : IFrameBehaviorInstance {
  public AnimationOneShotFrameBehavior Behavior;
  public Animator Animator;
  public void Initialize() {}
  public void OnStart() {}
  public void OnUpdate() {}
  public void OnLateUpdate() {}
  public void OnEnd() {}
  public void Cleanup() {}
}

public partial class AnimationOneShotFrameBehavior : IInstanceableRuntime {
  public IFrameBehaviorInstance CreateRuntimeInstance(object provider) {
    return new AnimationOneShotFrameBehaviorRuntime() {
      Behavior = this,
      Animator = TryGet<Animator>(provider, null)
    };
  }
}