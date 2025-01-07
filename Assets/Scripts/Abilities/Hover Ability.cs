using Abilities;
using UnityEngine;

public class HoverAbility : MonoBehaviour, IAbility {
  [Header("Reads From")]
  [SerializeField] AbilitySettings Settings;
  [Header("Writes To")]
  [SerializeField] Gravity Gravity;
  [SerializeField] Animator CharacterAnimator;
  [SerializeField] Animator WeaponAnimator;

  bool Active;

  public bool CanRun => true;

  public void Run() => Active = true;

  void FixedUpdate() {
    if (Active) {
      Gravity.FallingFactor.Set(Settings.HoverGravityFactor);
    }
    CharacterAnimator.SetBool("Hovering", Active);
    // This isn't really...correct. There once again probably should be some kind of
    // aggregator script for this thing that ultimately interacts with the weapon itself
    WeaponAnimator.SetInteger("Head State", Active ? 2 : 0);
    WeaponAnimator.SetBool("Spinning", Active);
    Active = false;
  }
}