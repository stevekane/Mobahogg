using System;
using System.ComponentModel;

[Serializable]
[DisplayName("Cancellable")]
public class CancellableFrameBehavior : FrameBehavior {
  ICancellable Cancellable;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Cancellable);
  }

  public override void OnStart() {
    Cancellable.Cancellable = true;
  }

  public override void OnEnd() {
    Cancellable.Cancellable = false;
  }
}
