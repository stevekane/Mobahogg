using UnityEngine;

[DefaultExecutionOrder(-100000)]
public class TimeManager : SingletonBehavior<TimeManager>, IClock {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void OnBoot() {
    // TODO: This isn't technically enough to lock update and fixed
    // together. you also need to disable vsync which is.. sketchy.
    // Consider what this might mean for how to do this "properly"
    Application.targetFrameRate = FPS;
    Time.fixedDeltaTime = 1f/FIXED_FPS;
    Debug.Log($"Application boot updateFPR:{FPS} | fixedFPS:{FIXED_FPS}");
  }

  static public int FIXED_FPS = 60;
  static public int FPS = 60;

  public IClock Parent() => null;
  [ContextMenu("Freeze")]
  public void Freeze() => IsFrozen = true;
  [ContextMenu("Unfreeze")]
  public void UnFreeze() => IsFrozen = false;
  public bool Frozen() => IsFrozen;
  public int FixedFrame() => TickCount;
  public float DeltaTime() => Frozen() ? 0 : Time.fixedDeltaTime;

  int TickCount;
  bool IsFrozen;

  void Start() {
    TickCount = 1;
  }

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