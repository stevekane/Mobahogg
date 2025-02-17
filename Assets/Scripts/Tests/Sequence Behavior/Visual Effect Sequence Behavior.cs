using UnityEngine;
using UnityEngine.VFX;
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