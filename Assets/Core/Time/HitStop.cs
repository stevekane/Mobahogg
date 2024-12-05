using UnityEngine;

public class HitStop : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;

  public int FramesRemaining;

  void Awake() {
    if (FramesRemaining > 0) {
      LocalClock.Freeze();
    }
  }

  void FixedUpdate() {
    if (LocalClock.Parent().Frozen())
      return;
    if (--FramesRemaining <= 0) {
      FramesRemaining = 0;
      LocalClock.UnFreeze();
    } else {
      LocalClock.Freeze();
    }
  }
}