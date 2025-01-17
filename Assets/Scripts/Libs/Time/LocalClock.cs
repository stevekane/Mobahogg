using UnityEngine;

[DefaultExecutionOrder(-100000)]
public class LocalClock : MonoBehaviour, IClock {
  public IClock Parent() => TimeManager.Instance;
  [ContextMenu("Freeze")]
  public void Freeze() => IsFrozen = true;
  [ContextMenu("Unfreeze")]
  public void UnFreeze() => IsFrozen = false;
  public bool Frozen() => Parent().Frozen() || IsFrozen;
  public int FixedFrame() => TickCount;
  public float Time() => time;
  public int DeltaFrames() => Frozen() ? 0 : 1;
  public float DeltaTime() => Frozen() ? 0 : Parent().DeltaTime();

  int TickCount = 0;
  float time = 0;
  bool IsFrozen;

  void Start() {
    TickCount = 1;
    time = 0;
  }

  void FixedUpdate() {
    if (!Frozen()) {
      time += DeltaTime();
      TickCount++;
    }
  }
}