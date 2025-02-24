using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviorTag {}

public interface IProvider<T> {
  public T Value(BehaviorTag tag);
}

public interface IConsumer {
  public abstract void Initialize(object provider);
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

  [Min(0)]
  public int StartFrame = 0;
  public int EndFrame = 1;
  public bool Starting(int frame) => frame == StartFrame;
  public bool Ending(int frame) => frame == EndFrame;
  public bool Active(int frame) => frame >= StartFrame && frame <= EndFrame;

  void OnValidate() => EndFrame = Mathf.Clamp(EndFrame, StartFrame, int.MaxValue);

  public virtual void Initialize(object provider) {}
  public virtual string Name => "Frame Behavior";
  public virtual void OnStart() {}
  public virtual void OnEnd() {}
  public virtual void OnUpdate() {}
}