using System;
using UnityEngine;

public class FrameBehaviorsPreviewProvider :
MonoBehaviour,
ICancellable,
ITypeAndTagProvider<BehaviorTag>
{
  [SerializeField] Animator Animator;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] AudioSource AudioSource;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] WeaponAim WeaponAim;
  [SerializeField] Hitbox Hitbox;
  [SerializeField] Vibrator Vibrator;
  [SerializeField] GameObject Owner;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] BehaviorTag WeaponButtTag;
  [SerializeField, Tooltip("Rear end of weapon")] Transform WeaponButt;
  [SerializeField] BehaviorTag OwnerTag;

  public bool Cancellable { get; set; }

  public object Get(Type type, BehaviorTag tag) => (type, tag) switch {
    _ when type == typeof(GameObject) => gameObject,
    _ when type == typeof(Animator) => Animator,
    _ when type == typeof(AnimatorCallbackHandler) => AnimatorCallbackHandler,
    _ when type == typeof(KCharacterController) => CharacterController,
    _ when type == typeof(WeaponAim) => WeaponAim,
    _ when type == typeof(Transform) & tag == WeaponButtTag => WeaponButt,
    _ when type == typeof(Transform) => transform,
    _ when type == typeof(LocalClock) => LocalClock,
    _ when type == typeof(Vibrator) => Vibrator,
    _ when type == typeof(ICancellable) => this,
    _ => null
  };
}