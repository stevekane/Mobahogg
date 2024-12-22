using UnityEngine;
using UnityEngine.Playables;

public class MoveTweenBehavior : PlayableBehaviour {
  public Transform Target;
  public Vector3 StartPosition;
  public Vector3 EndPosition;
  public AnimationCurve XCurve = AnimationCurve.Linear(0, 0, 1, 1);
  public AnimationCurve YCurve = AnimationCurve.Linear(0, 0, 1, 1);
  public AnimationCurve ZCurve = AnimationCurve.Linear(0, 0, 1, 1);

  public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
    float progress = (float)(playable.GetTime() / playable.GetDuration());
    float x = Mathf.Lerp(StartPosition.x, EndPosition.x, XCurve.Evaluate(progress));
    float y = Mathf.Lerp(StartPosition.y, EndPosition.y, YCurve.Evaluate(progress));
    float z = Mathf.Lerp(StartPosition.z, EndPosition.z, ZCurve.Evaluate(progress));
    Target.position = new(x,y,z);
  }
}