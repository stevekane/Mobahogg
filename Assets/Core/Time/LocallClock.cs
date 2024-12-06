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
  public float DeltaTime() => Frozen() ? 0 : Parent().DeltaTime();
  public int DeltaFrames() => Frozen() ? 0 : 1;

  int TickCount = 0;
  bool IsFrozen;

  void Start() {
    TickCount = 1;
  }

  void FixedUpdate() {
    if (!Frozen())
      TickCount++;
  }
}