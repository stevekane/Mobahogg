using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;




#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IConsumer {
  public abstract void Initialize(object provider);
  #if UNITY_EDITOR
  public abstract void PreviewInitialize(object provider);
  #endif
}

public static class FrameBehaviorExtensions {
  public static async UniTask RunInstance(
  FrameBehaviors frameBehaviors,
  ITypeAndTagProvider<FrameBehavior> provider,
  LocalClock localClock,
  CancellationToken token) {
    var instance = new FrameBehaviors(frameBehaviors);
    var frame = 0;
    var endFrame = instance.EndFrame;
    try {
      instance.Behaviors.ForEach(behavior => behavior.Initialize(provider));
      do {
        if (!localClock.Frozen()) {
          FrameBehavior.StartBehaviors(instance.Behaviors, frame);
          FrameBehavior.UpdateBehaviors(instance.Behaviors, frame);
          FrameBehavior.EndBehaviors(instance.Behaviors, frame);
          frame = frame + localClock.DeltaFrames();
        }
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
      } while (frame <= endFrame);
    } catch (Exception e) {
      throw e;
    } finally {
      FrameBehavior.EndBehaviors(instance.Behaviors, frame);
    }
  }
}

[Serializable]
public abstract class FrameBehavior : IConsumer {
  public static bool TryGetValue<T>(object provider, BehaviorTag tag, out T t) {
    var tProvider = provider as ITypeAndTagProvider<BehaviorTag>;
    if (tProvider != null) {
      t = (T)tProvider.Get(typeof(T), tag);
      return true;
    } else {
      t = default;
      return false;
    }
  }

  public static FrameBehavior Clone(FrameBehavior frameBehavior) => frameBehavior.Clone();

  public static T TryGet<T>(object provider, BehaviorTag tag) {
    var tProvider = provider as ITypeAndTagProvider<BehaviorTag>;
    return tProvider != null
      ? (T)tProvider.Get(typeof(T), tag)
      : default;
  }

  public static void InitializeBehaviors(IEnumerable<FrameBehavior> behaviors, object provider) =>
    behaviors
    .ForEach(b => b.Initialize(provider));

  public static void StartBehaviors(IEnumerable<FrameBehavior> behaviors, int frame) =>
    behaviors
    .Where(b => b.Starting(frame))
    .ForEach(b => b.OnStart());

  public static void EndBehaviors(IEnumerable<FrameBehavior> behaviors, int frame) =>
    behaviors
    .Where(b => b.Ending(frame))
    .ForEach(b => b.OnEnd());

  public static void UpdateBehaviors(IEnumerable<FrameBehavior> behaviors, int frame) =>
    behaviors
    .Where(b => b.Active(frame))
    .ForEach(b => b.OnUpdate());

  public static void CancelActiveBehaviors(IEnumerable<FrameBehavior> behaviors, int frame) =>
    behaviors
    .Where(b => b.Active(frame))
    .ForEach(b => b.OnEnd());

  #if UNITY_EDITOR
  public static void PreviewInitializeBehaviors(IEnumerable<FrameBehavior> behaviors, object provider) {
    foreach (var behavior in behaviors) {
      behavior.PreviewInitialize(provider);
    }
  }

  public static void PreviewCleanupBehaviors(IEnumerable<FrameBehavior> behaviors, object provider) {
    foreach (var behavior in behaviors) {
      behavior.PreviewCleanup(provider);
    }
  }

  public static void PreviewStartBehaviors(IEnumerable<FrameBehavior> behaviors, int frame, PreviewRenderUtility preview) {
    foreach (var behavior in behaviors) {
      if (behavior.Starting(frame)) {
        behavior.PreviewOnStart(preview);
      }
    }
  }

  public static void PreviewEndBehaviors(IEnumerable<FrameBehavior> behaviors, int frame, PreviewRenderUtility preview) {
    foreach (var behavior in behaviors) {
      if (behavior.Ending(frame)) {
        behavior.PreviewOnEnd(preview);
      }
    }
  }

  public static void PreviewUpdateBehaviors(IEnumerable<FrameBehavior> behaviors, int frame, PreviewRenderUtility preview) {
    foreach (var behavior in behaviors) {
      if (behavior.Active(frame)) {
        behavior.PreviewOnUpdate(preview);
      }
    }
  }

  public static void PreviewLateUpdateBehaviors(IEnumerable<FrameBehavior> behaviors, int frame, PreviewRenderUtility preview) {
    foreach (var behavior in behaviors) {
      if (behavior.Active(frame)) {
        behavior.PreviewOnLateUpdate(preview);
      }
    }
  }

  public static void PreviewCancelActiveBehaviors(IEnumerable<FrameBehavior> behaviors, int frame, PreviewRenderUtility preview) {
    foreach (var behavior in behaviors) {
      if (behavior.Active(frame)) {
        behavior.PreviewOnEnd(preview);
      }
    }
  }
  #endif

  [Min(0)]
  public int StartFrame = 0;
  public int EndFrame = 1;
  public bool Starting(int frame) => frame == StartFrame;
  public bool Ending(int frame) => frame == EndFrame;
  public bool Active(int frame) => frame >= StartFrame && frame <= EndFrame;

  public virtual void Initialize(object provider) {}
  public virtual void Cleanup(object provider) {}
  public virtual void OnStart() {}
  public virtual void OnEnd() {}
  public virtual void OnUpdate() {}
  public virtual FrameBehavior Clone() => (FrameBehavior)MemberwiseClone();
  #if UNITY_EDITOR
  public bool ShowPreview = true;
  void OnValidate() => EndFrame = Mathf.Clamp(EndFrame, StartFrame, int.MaxValue);
  public virtual void PreviewInitialize(object provider) {}
  public virtual void PreviewCleanup(object provider) {}
  public virtual void PreviewOnStart(PreviewRenderUtility preview) {}
  public virtual void PreviewOnUpdate(PreviewRenderUtility preview) {}
  public virtual void PreviewOnLateUpdate(PreviewRenderUtility preview) {}
  public virtual void PreviewOnEnd(PreviewRenderUtility preview) {}
  #endif
}