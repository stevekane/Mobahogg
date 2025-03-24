using UnityEngine;
using System;
using System.ComponentModel;


#if UNITY_EDITOR
using UnityEditor;
public partial class VibrateFrameBehavior {
  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, null, out Vibrator);
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    Vibrator.StartVibrate(Axis, EndFrame-StartFrame, Intensity, Frequency);
  }

  public override void PreviewOnLateUpdate(PreviewRenderUtility preview) {
    Vibrator.Vibrate(Time.fixedDeltaTime);
  }
}
#endif

[Serializable]
[DisplayName("Vibrate")]
public partial class VibrateFrameBehavior : FrameBehavior {
  public Vector3 Axis = Vector3.up;
  public float Frequency = 20;
  public float Intensity = 0.125f;

  Vibrator Vibrator;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Vibrator);
  }

  public override void OnStart() {
    if (Vibrator)
      Vibrator.StartVibrate(Axis, EndFrame-StartFrame, Intensity, Frequency);
  }

  public VibrateInstance Instantiate(ITypeAndTagProvider<FrameBehavior> provider) {
    return new VibrateInstance {
      FrameBehavior = this,
      Vibrator = provider.Get<Vibrator>(default)
    };
  }
}

public class VibrateInstance {
  public VibrateFrameBehavior FrameBehavior;
  public Vibrator Vibrator;

  public void OnStart() {
    Vibrator.StartVibrate(
      FrameBehavior.Axis,
      FrameBehavior.EndFrame-FrameBehavior.StartFrame,
      FrameBehavior.Intensity,
      FrameBehavior.Frequency);
  }
}

public class VibratePreview {
  public VibrateFrameBehavior FrameBehavior;
  public Vibrator Vibrator;

  public void OnStart() {
    Vibrator.StartVibrate(
      FrameBehavior.Axis,
      FrameBehavior.EndFrame-FrameBehavior.StartFrame,
      FrameBehavior.Intensity,
      FrameBehavior.Frequency);
  }
}