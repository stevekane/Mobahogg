using UnityEngine.Timeline;
using UnityEngine.VFX;

[TrackColor(0.2f, 0.8f, 0.3f)]
public class VisualEffectSequence : SequenceBehavior {
  public VisualEffectAsset EffectAsset;
  public SceneBinding<VisualEffect> VisualEffect;
  public string StartEventName;
  public string EndEventName;
  public string UpdateEventName;

  public override string Name => EffectAsset
    ? EffectAsset.name
    : "Visual Effect";

  public override void OnStart() {
    // VisualEffect.SendEvent(StartEventName);
  }

  public override void OnEnd() {
    // VisualEffect.SendEvent(EndEventName);
  }

  public override void OnFrame() {
    // VisualEffect.SendEvent(UpdateEventName);
  }
}