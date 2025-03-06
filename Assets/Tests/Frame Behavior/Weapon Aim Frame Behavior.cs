using System;
using System.ComponentModel;
using UnityEngine;

[Serializable]
[DisplayName("Aim Weapon")]
public class WeaponAimFrameBehavior : FrameBehavior {
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
