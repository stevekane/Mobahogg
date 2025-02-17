using UnityEngine;
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