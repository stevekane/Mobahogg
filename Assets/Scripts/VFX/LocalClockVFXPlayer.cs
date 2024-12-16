using UnityEngine;
using UnityEngine.VFX;

public class LocalClockVFXPlayer : MonoBehaviour {
  [SerializeField] VisualEffect VisualEffect;
  [SerializeField] LocalClock LocalClock;

  void FixedUpdate() {
    VisualEffect.pause = LocalClock.Frozen();
  }
}