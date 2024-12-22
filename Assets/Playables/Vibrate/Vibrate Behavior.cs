using UnityEngine;
using UnityEngine.Playables;

public class VibrateBehavior : PlayableBehaviour {
  public float FrequencyStart;
  public float FrequencyEnd;
  public AnimationCurve FrequencyAnimationCurve;
  public float AmplitudeStart;
  public float AmplitudeEnd;
  public AnimationCurve AmplitudeAnimationCurve;
  public float AxisChangeRate;

  public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
    var transform = playerData as Transform;
    var time = (float)playable.GetTime();
    var interpolant = time / (float)playable.GetDuration();
    var frequency = Mathf.Lerp(
      FrequencyStart,
      FrequencyEnd,
      FrequencyAnimationCurve.Evaluate(interpolant));
    var amplitude = Mathf.Lerp(
      AmplitudeStart,
      AmplitudeEnd,
      AmplitudeAnimationCurve.Evaluate(interpolant));
    var displacement = amplitude * Mathf.Sin(time * frequency * 2.0f * Mathf.PI);
    var axis = new Vector3(
      Mathf.Sin(time * AxisChangeRate),
      Mathf.Cos(time * AxisChangeRate),
      Mathf.Tan(time * AxisChangeRate));
    var localAxis = transform.InverseTransformDirection(axis.normalized);
    transform.localPosition = displacement * localAxis;
  }
}