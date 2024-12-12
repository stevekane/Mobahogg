using UnityEngine;

public class LocalClockSpinner : MonoBehaviour {
  public float DegreesPerSecond = 360;

  [SerializeField] LocalClock Clock;

  void FixedUpdate() {
    transform.Rotate(transform.up, Clock.DeltaTime() * DegreesPerSecond);
  }
}