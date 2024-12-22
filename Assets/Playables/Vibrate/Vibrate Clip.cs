using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class VibrateClip : PlayableAsset {
  public float FrequencyStart = 20;
  public float FrequencyEnd = 20;
  public AnimationCurve FrequencyAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
  public float AmplitudeStart = 1f;
  public float AmplitudeEnd = 1;
  public AnimationCurve AmplitudeAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
  public float AxisChangeRate = 10;

  public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
    var playable = ScriptPlayable<VibrateBehavior>.Create(graph);
    var behaviour = playable.GetBehaviour();

    behaviour.FrequencyStart = FrequencyStart;
    behaviour.FrequencyEnd = FrequencyEnd;
    behaviour.AmplitudeStart = AmplitudeStart;
    behaviour.FrequencyAnimationCurve = FrequencyAnimationCurve;
    behaviour.AmplitudeEnd = AmplitudeEnd;
    behaviour.AxisChangeRate = AxisChangeRate;
    behaviour.AmplitudeAnimationCurve = AmplitudeAnimationCurve;
    return playable;
  }
}