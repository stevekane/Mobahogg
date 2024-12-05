using UnityEngine;

[DefaultExecutionOrder(1000)]
public class LocalClockAnimatorBrain : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;

  void Awake() {
    Animator.enabled = false;
  }

  void FixedUpdate() {
    Animator.Update(LocalClock.DeltaTime());
  }
}