using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class MoveTweenClip : PlayableAsset {
  public ExposedReference<Transform> Target;
  public Vector3 LocalTravelDelta;
  public AnimationCurve XCurve = AnimationCurve.Linear(0, 0, 1, 1);
  public AnimationCurve YCurve = AnimationCurve.Linear(0, 0, 1, 1);
  public AnimationCurve ZCurve = AnimationCurve.Linear(0, 0, 1, 1);

  public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
    var playable = ScriptPlayable<MoveTweenBehavior>.Create(graph);
    var behaviour = playable.GetBehaviour();

    behaviour.Target = Target.Resolve(graph.GetResolver());
    behaviour.StartPosition = Vector3.zero;
    behaviour.EndPosition = LocalTravelDelta;
    behaviour.XCurve = XCurve;
    behaviour.YCurve = YCurve;
    behaviour.ZCurve = ZCurve;

    return playable;
  }
}