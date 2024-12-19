using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class SpinClip : PlayableAsset {
  public float Revolutions = 10;
  public AnimationCurve RotationCurve = AnimationCurve.Linear(0,0,1,1);
  public Vector3 Axis = Vector3.up;

  public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
    var playable = ScriptPlayable<SpinBehavior>.Create(graph);
    var behaviour = playable.GetBehaviour();
    behaviour.Revolutions = Revolutions;
    behaviour.RotationCurve = RotationCurve;
    behaviour.Axis = Axis;
    return playable;
  }
}