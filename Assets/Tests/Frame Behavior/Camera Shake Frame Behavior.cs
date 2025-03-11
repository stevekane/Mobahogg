using UnityEngine;
using System;
using System.ComponentModel;

#if UNITY_EDITOR
using UnityEditor;
public partial class CameraShakeFrameBehavior {
  // Kind of a hack to bridge between runtime behavior from CameraShaker and what you see here.
  // TODO: This could all be made more precise/accurate if desired
  const float INTENSITY_SCALAR = 0.1f;
  const float SHAKE_DECAY_EPSILON = -2f;

  System.Random RNG;
  float CurrentIntensity;

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    RNG = new(0);
    CurrentIntensity = Intensity;
    preview.camera.transform.localPosition = Vector3.zero;
  }

  public override void PreviewOnLateUpdate(PreviewRenderUtility preview) {
    CurrentIntensity = Mathf.Lerp(0, CurrentIntensity, Mathf.Exp(Time.fixedDeltaTime*SHAKE_DECAY_EPSILON));
    preview.camera.transform.localPosition = INTENSITY_SCALAR * CurrentIntensity * new Vector3(
      (float)RNG.NextDouble(),
      (float)RNG.NextDouble(),
      0);
  }

  public override void PreviewOnEnd(PreviewRenderUtility preview) {
    preview.camera.transform.localPosition = Vector3.zero;
  }
}
#endif

[Serializable]
[DisplayName("Camera Shake")]
public partial class CameraShakeFrameBehavior : FrameBehavior {
  public float Intensity = 1;

  public override void OnStart() {
    CameraManager.Instance.Shake(Intensity);
  }
}