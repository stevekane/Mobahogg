using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.VFX;

[TrackColor(0.5f, 0.7f,.9f)]
public class VisualEffectSequence : SequenceBehavior {
  public VisualEffect effect;

  public override string Name =>
    effect && effect.visualEffectAsset
      ? effect.visualEffectAsset.name
      : "Visual Effect";

  public override void OnStart() {

  }

  public override void OnEnd() {

  }

  public override void OnFrame() {

  }
}