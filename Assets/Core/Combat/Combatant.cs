using UnityEngine;

public class Combatant : MonoBehaviour {
  [SerializeField] int HitStopFrames = 4;
  [SerializeField] string HitFlinchName = "Hit Flinch";
  [SerializeField] string HurtFlinchName = "Hurt Flinch";
  [SerializeField] HitStop HitStop;
  [SerializeField] Animator Animator;

  void OnHit(Combatant victim) {
    HitStop.FramesRemaining = HitStopFrames;
    if (Animator && HitFlinchName != "") Animator.SetTrigger(HitFlinchName);
  }

  void OnHurt(Combatant attacker) {
    HitStop.FramesRemaining = HitStopFrames;
    if (Animator && HurtFlinchName != "") Animator.SetTrigger(HurtFlinchName);
  }
}