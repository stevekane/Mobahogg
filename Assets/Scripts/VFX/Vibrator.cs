using UnityEngine;

/*
TODO:
Multiple things could try to write to this in a given frame so perhaps it should
have the concurrent update structure that we've come to favor so heavily.
*/
[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class Vibrator : MonoBehaviour {
  [SerializeField] Transform Target;
  [SerializeField] LocalClock LocalClock;

  Vector3 LocalPosition;
  public Vector3 Axis;
  public float Frequency;
  public float Amplitude;
  float ElapsedTime;
  int FramesRemaining;

  public void Vibrate(Vector3 axis, int frames, float amplitude, float frequency) {
    Axis = axis;
    Amplitude = Mathf.Abs(amplitude);
    Frequency = frequency;
    ElapsedTime = 0;
    FramesRemaining = Mathf.Max(FramesRemaining, frames);
  }

  void Start() {
    LocalPosition = Target.transform.localPosition;
  }

  void FixedUpdate() {
    if (FramesRemaining > 0) {
      var displacement = Mathf.Sin(ElapsedTime * Frequency * 2.0f * Mathf.PI) * Amplitude;
      var localAxis = Target.transform.InverseTransformDirection(Axis);
      Target.transform.localPosition = LocalPosition + displacement * localAxis;
      ElapsedTime += LocalClock.Parent().DeltaTime();
      FramesRemaining--;
    } else {
      FramesRemaining = 0;
      ElapsedTime = 0;
      Target.transform.localPosition = LocalPosition;
    }
  }
}