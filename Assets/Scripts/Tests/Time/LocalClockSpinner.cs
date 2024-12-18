using UnityEngine;

public class LocalClockSpinner : MonoBehaviour {
  public float DegreesPerSecond = 360;

  [SerializeField] LocalClock Clock;

  void FixedUpdate() {
    transform.Rotate(Vector3.up, Clock.DeltaTime() * DegreesPerSecond);
  }
}