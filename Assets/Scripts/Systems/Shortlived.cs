using UnityEngine;

public class Shortlived : MonoBehaviour {
  public Timeval Lifetime = Timeval.FromSeconds(10);

  int Frame;

  void FixedUpdate() {
    if (Frame >= Lifetime.Ticks) {
      Destroy(gameObject);
    }

    if (TryGetComponent(out LocalClock localClock)) {
      Frame += localClock.DeltaFrames();
    } else {
      Frame += 1;
    }
  }
}