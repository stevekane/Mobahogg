using UnityEngine;

public class SpinAbility : MonoBehaviour {
  [SerializeField] AbilitySettings AbilitySettings;
  [SerializeField] Player Player;
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] AttackAbility AttackAbility;

  int FramesRemaining;

  public bool CanRun() => !Player.IsDashing() && !AttackAbility.IsRunning;
  public bool TryRun() {
    if (CanRun()) {
      FramesRemaining = AbilitySettings.TotalSpinFrames;
      return true;
    } else {
      return false;
    }
  }
  public void Cancel() {
    FramesRemaining = 0;
  }

  void FixedUpdate() {
    FramesRemaining = Mathf.Max(0, FramesRemaining-LocalClock.DeltaFrames());
    Animator.SetBool("Spinning", FramesRemaining > 0);
  }
}