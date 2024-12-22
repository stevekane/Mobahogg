using UnityEngine;
using UnityEngine.Playables;

public class SpinBehavior : PlayableBehaviour {
  public float Revolutions;
  public AnimationCurve RotationCurve;
  public Vector3 Axis;

  public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
    var transform = playerData as Transform;
    var time = (float)playable.GetTime();
    var duration = (float)playable.GetDuration();
    var interpolant = RotationCurve.Evaluate(time / duration);
    var angle = Mathf.Lerp(0, Revolutions * 360, interpolant);
    transform.localRotation = Quaternion.AngleAxis(angle, Axis);
  }
}