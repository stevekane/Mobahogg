using State;
using UnityEngine;

public class HitStop : AbstractState {
  [SerializeField] LocalClock LocalClock;

  int Accumulator;
  int Current;

  public int FramesRemaining { get => Current; set => Accumulator = Mathf.Max(Accumulator, value); }

  void Start() {
    if (FramesRemaining > 0) {
      LocalClock.Freeze();
    }
  }

  void FixedUpdate() {
    Current = Accumulator;
    Accumulator = 0;

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