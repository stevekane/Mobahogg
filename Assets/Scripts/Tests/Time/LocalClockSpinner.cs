using UnityEngine;

public class LocalClockSpinner : MonoBehaviour {
  [SerializeField] LocalClock Clock;
  [SerializeField] float DegreesPerSecond = 360;

  void FixedUpdate() {
    transform.Rotate(transform.up, Clock.DeltaTime() * DegreesPerSecond);
  }
}