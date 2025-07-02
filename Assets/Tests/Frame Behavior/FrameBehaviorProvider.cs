using System;
using UnityEngine;

public class FrameBehaviorProvider :
MonoBehaviour,
ITypeAndTagProvider<BehaviorTag>
{
  [SerializeField] Animator Animator;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] WeaponAim WeaponAim;
  [SerializeField] Vibrator Vibrator;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] BehaviorTag WeaponTipTag;
  [SerializeField] BehaviorTag WeaponButtTag;
  [SerializeField, Tooltip("Tip end of weapon")] Transform WeaponTip;
  [SerializeField, Tooltip("Rear end of weapon")] Transform WeaponButt;
  [SerializeField] SpellStaff Weapon;
  [SerializeField] BehaviorTag OwnerTag;

  public object Get(Type type, BehaviorTag tag) => (type, tag) switch {
    _ when type == typeof(GameObject) => gameObject,
    _ when type == typeof(Animator) => Animator,
    _ when type == typeof(AnimatorCallbackHandler) => AnimatorCallbackHandler,
    _ when type == typeof(KCharacterController) => CharacterController,
    _ when type == typeof(WeaponAim) => WeaponAim,
    _ when type == typeof(Transform) & tag == WeaponButtTag => WeaponButt,
    _ when type == typeof(Transform) & tag == WeaponTipTag => WeaponTip,
    _ when type == typeof(Transform) => transform,
    _ when type == typeof(LocalClock) => LocalClock,
    _ when type == typeof(Vibrator) => Vibrator,
    _ when type == typeof(SpellStaff) => Weapon,
    _ => null
  };
}