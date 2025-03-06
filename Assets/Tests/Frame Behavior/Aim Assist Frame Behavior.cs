using System;
using System.ComponentModel;
using UnityEngine;
using AimAssist;

[Serializable]
[DisplayName("Aim Assist")]
public class AimAssistFrameBehavior : FrameBehavior {
  public float TurnSpeed = 90;
  [InlineEditor]
  public AimAssistQuery AimAssistQuery;

  KCharacterController CharacterController;
  LocalClock LocalClock;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out CharacterController);
    TryGetValue(provider, null, out LocalClock);
  }

  public override void OnUpdate() {
    var bestTarget = AimAssistManager.Instance.BestTarget(
      CharacterController.transform,
      AimAssistQuery);
    if (bestTarget) {
      var direction = bestTarget.transform.position-CharacterController.transform.position;
      var maxDegrees = TurnSpeed * LocalClock.DeltaTime();
      var nextRotation = Quaternion.RotateTowards(
        CharacterController.transform.rotation,
        Quaternion.LookRotation(direction),
        maxDegrees);
      CharacterController.Rotation.Set(nextRotation);
    }
  }
}
