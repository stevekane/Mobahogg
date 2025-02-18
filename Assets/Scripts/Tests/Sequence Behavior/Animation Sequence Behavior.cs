using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(0.25f, .15f, .25f)]
public class AnimationSequence : SequenceBehavior {
  public AnimationClip clip;

  public override string Name => clip ? clip.name : "Animation";

  public override void OnStart() {

  }

  public override void OnEnd() {

  }

  public override void OnFrame() {

  }
}