using System;
using System.ComponentModel;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public partial class WeaponAimFrameBehavior {
  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, null, out WeaponAim);
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    WeaponAim.AimDirection = Direction;
  }

  public override void PreviewOnEnd(PreviewRenderUtility preview) {
    WeaponAim.AimDirection = null;
  }

  public override void PreviewOnLateUpdate(PreviewRenderUtility preview) {
    WeaponAim.Aim(1/60f);
  }
}
#endif

[Serializable]
[DisplayName("Aim Weapon")]
public partial class WeaponAimFrameBehavior : FrameBehavior {
  public Vector3 Direction;

  WeaponAim WeaponAim;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out WeaponAim);
  }

  public override void OnStart() {
    WeaponAim.AimDirection = Direction;
  }

  public override void OnEnd() {
    WeaponAim.AimDirection = null;
  }
}