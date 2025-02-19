using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(0.9f, 0.55f, 0.2f)]
public class AnimationSequence : SequenceBehavior {
  public AnimationClip clip;
  public SceneBinding<Animator> Animator;

  public override string Name => clip ? clip.name : "Animation";

  public override void OnStart() {

  }

  public override void OnEnd() {

  }

  public override void OnFrame() {

  }
}