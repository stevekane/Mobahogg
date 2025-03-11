using UnityEngine;

public class FrameBehaviorsPreviewProvider :
MonoBehaviour,
ICancellable,
IProvider<Animator>,
IProvider<AnimatorCallbackHandler>,
IProvider<AudioSource>,
IProvider<KCharacterController>,
IProvider<WeaponAim>,
IProvider<Hitbox>,
IProvider<Vibrator>,
IProvider<GameObject>,
IProvider<Transform>,
IProvider<LocalClock>,
IProvider<ICancellable>
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

  Animator IProvider<Animator>.Value(BehaviorTag tag) => Animator;
  AnimatorCallbackHandler IProvider<AnimatorCallbackHandler>.Value(BehaviorTag tag) => AnimatorCallbackHandler;
  AudioSource IProvider<AudioSource>.Value(BehaviorTag tag) => AudioSource;
  KCharacterController IProvider<KCharacterController>.Value(BehaviorTag tag) => CharacterController;
  WeaponAim IProvider<WeaponAim>.Value(BehaviorTag tag) => WeaponAim;
  GameObject IProvider<GameObject>.Value(BehaviorTag tag) => Owner;
  Transform IProvider<Transform>.Value(BehaviorTag tag) => tag == WeaponButtTag ? WeaponButt : transform;
  LocalClock IProvider<LocalClock>.Value(BehaviorTag tag) => LocalClock;
  Hitbox IProvider<Hitbox>.Value(BehaviorTag tag) => Hitbox;
  Vibrator IProvider<Vibrator>.Value(BehaviorTag tag) => Vibrator;
  ICancellable IProvider<ICancellable>.Value(BehaviorTag tag) => this;
}