using UnityEngine;

[DefaultExecutionOrder(-2)] // Right before Animator which supposedly runs at -1
public class LocalClockAnimatorBrain : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;

  void Start() {
    Animator.updateMode = AnimatorUpdateMode.Fixed;
  }

  void FixedUpdate() {
    Animator.enabled = !LocalClock.Frozen();
  }
}