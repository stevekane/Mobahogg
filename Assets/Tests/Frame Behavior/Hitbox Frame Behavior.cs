using System;
using System.ComponentModel;

[Serializable]
[DisplayName("Hitbox")]
public class HitboxBehavior : FrameBehavior {
  Hitbox Hitbox;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Hitbox);
  }

  public override void OnStart() {
    Hitbox.CollisionEnabled = true;
  }

  public override void OnEnd() {
    Hitbox.CollisionEnabled = false;
  }
}
