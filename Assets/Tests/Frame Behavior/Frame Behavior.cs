using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class BehaviorTag {}

public interface IProvider<T> {
  public T Value(BehaviorTag tag);
}

public interface IConsumer {
  public abstract void Initialize(object provider);
  #if UNITY_EDITOR
  public abstract void PreviewInitialize(object provider);
  #endif
}

[Serializable]
public abstract class FrameBehavior : IConsumer {
  public static bool TryGetValue<T>(object provider, BehaviorTag tag, out T t) {
    var tProvider = provider as IProvider<T>;
    if (tProvider != null) {
      t = tProvider.Value(tag);
      return true;
    } else {
      t = default;
      return false;
    }
  }

  public static T TryGet<T>(object provider, BehaviorTag tag) {
    var tProvider = provider as IProvider<T>;
    return tProvider != null
      ? tProvider.Value(tag)
      : default;
  }

  public static void InitializeBehaviors(IEnumerable<FrameBehavior> behaviors, object provider) {
    foreach (var behavior in behaviors) {
      behavior.Initialize(provider);
    }
  }

  public static void StartBehaviors(IEnumerable<FrameBehavior> behaviors, int frame) {
    foreach (var behavior in behaviors) {
      if (behavior.Starting(frame)) {
        behavior.OnStart();
      }
    }
  }

  public static void EndBehaviors(IEnumerable<FrameBehavior> behaviors, int frame) {
    foreach (var behavior in behaviors) {
      if (behavior.Ending(frame)) {
        behavior.OnEnd();
      }
    }
  }

  public static void UpdateBehaviors(IEnumerable<FrameBehavior> behaviors, int frame) {
    foreach (var behavior in behaviors) {
      if (behavior.Active(frame)) {
        behavior.OnUpdate();
      }
    }
  }

  public static void CancelActiveBehaviors(IEnumerable<FrameBehavior> behaviors, int frame) {
    foreach (var behavior in behaviors) {
      if (behavior.Active(frame)) {
        behavior.OnEnd();
      }
    }
  }

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
  public virtual void OnStart() {}
  public virtual void OnEnd() {}
  public virtual void OnUpdate() {}
  public virtual FrameBehavior Clone() {
    return (FrameBehavior)MemberwiseClone();
  }
  #if UNITY_EDITOR
  void OnValidate() => EndFrame = Mathf.Clamp(EndFrame, StartFrame, int.MaxValue);
  public virtual void PreviewInitialize(object provider) {}
  public virtual void PreviewCleanup(object provider) {}
  public virtual void PreviewOnStart(PreviewRenderUtility preview) {}
  public virtual void PreviewOnUpdate(PreviewRenderUtility preview) {}
  public virtual void PreviewOnEnd(PreviewRenderUtility preview) {}
  #endif
}