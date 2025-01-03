using UnityEngine;
using UnityEngine.Events;

public class JumpPad : MonoBehaviour {
  [SerializeField] JumpPadSettings Settings;
  [SerializeField] UnityEvent OnTrigger;

  void OnTriggerEnter(Collider c) {
    if (c.TryGetComponent(out Player player) && c.TryGetComponent(out KCharacterController controller)) {
      controller.ForceUnground.Set(true);
      controller.Velocity.SetY(Settings.JumpStrength);
      OnTrigger?.Invoke();
    }
  }
}