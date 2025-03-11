using System;
using System.ComponentModel;

#if UNITY_EDITOR
using UnityEditor;
public partial class CameraShakeFrameBehavior {
  // todo: Preview
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