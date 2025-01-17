using UnityEngine;

[DefaultExecutionOrder(-100000)]
public class TimeManager : SingletonBehavior<TimeManager>, IClock {
  public IClock Parent() => null;
  [ContextMenu("Freeze")]
  public void Freeze() => IsFrozen = true;
  [ContextMenu("Unfreeze")]
  public void UnFreeze() => IsFrozen = false;
  public bool Frozen() => IsFrozen;
  public int FixedFrame() => TickCount;
  public float DeltaTime() => Frozen() ? 0 : Time.fixedDeltaTime;
  public void Log(string msg) => Debug.Log($"{FixedFrame()}: {msg}");

  int TickCount;
  bool IsFrozen;

  void FixedUpdate() {
    if (!Frozen())
      TickCount++;
  }
}

public interface IClock {
  public IClock Parent();
  public void Freeze();
  public void UnFreeze();
  public bool Frozen();
  public int FixedFrame();
  public float DeltaTime();
}