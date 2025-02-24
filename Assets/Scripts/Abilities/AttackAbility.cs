using System.Collections.Generic;
using UnityEngine;
using AimAssist;
using Abilities;
using System;
using UnityEngine.VFX;

interface IProvider<T> {
  public T Value(BehaviorTag tag);
}

interface IConsumer {
  public abstract void Initialize(object potentialProvider);
}

abstract class BehaviorTag {}

class WeaponTag : BehaviorTag {}
class PlayerTag : BehaviorTag {}

class AnimatorConsumer : IConsumer {
  BehaviorTag Tag;
  Animator Animator;

  public void Initialize(object potentialProvider) {
    if (potentialProvider is IProvider<Animator> animatorProvider) {
      Animator = animatorProvider.Value(Tag);
    }
  }
}

class CompositeConsumer : IConsumer {
  BehaviorTag OwnerTag;
  BehaviorTag AnimatorTag;

  Animator Animator;
  GameObject Owner;

  public void Initialize(object potentialProvider) {
    var animatorProvider = potentialProvider as IProvider<Animator>;
    var gameObjectProvier = potentialProvider as IProvider<GameObject>;
    if (animatorProvider != null && gameObjectProvier != null) {
      Animator = animatorProvider.Value(AnimatorTag);
      Owner = gameObjectProvier.Value(OwnerTag);
    }
  }
}

class Host : MonoBehaviour, IProvider<Animator>, IProvider<GameObject> {
  List<IConsumer> Consumers = new();

  WeaponTag WeaponTag;
  PlayerTag PlayerTag;
  Animator WeaponAnimator;
  Animator PlayerAnimator;

  Animator IProvider<Animator>.Value(BehaviorTag tag) => tag switch {
    var t when t == PlayerTag => PlayerAnimator,
    var t when t == WeaponTag => WeaponAnimator,
    _ => null
  };

  GameObject IProvider<GameObject>.Value(BehaviorTag tag) => gameObject;

  public void Run() {
    Consumers.ForEach(c => c.Initialize(this));
  }
}

[Serializable]
public class RootMotionBehavior : FrameBehavior {
  public float Multiplier;
  AnimatorCallbackHandler AnimatorCallbackHandler;
  KCharacterController CharacterController;
  LocalClock LocalClock;

  void OnRootMotion() {
    var forward = CharacterController.Rotation.Forward;
    var dp = Vector3.Project(AnimatorCallbackHandler.Animator.deltaPosition, forward);
    var v = dp / LocalClock.DeltaTime();
    CharacterController.DirectVelocity.Add(Multiplier * v);
  }

  public override void OnStart(GameObject runner, GameObject owner) {
    LocalClock = owner.GetComponent<LocalClock>();
    CharacterController = owner.GetComponent<KCharacterController>();
    AnimatorCallbackHandler = owner.GetComponent<AbilityManager>().LocateComponent<AnimatorCallbackHandler>();
    AnimatorCallbackHandler.OnRootMotion.Listen(OnRootMotion);
  }

  public override void OnEnd(GameObject runner, GameObject owner) {
    AnimatorCallbackHandler.OnRootMotion.Unlisten(OnRootMotion);
  }
}

[Serializable]
public class AimAssistBehavior : FrameBehavior {
  public float TurnSpeed = 90;
  [InlineEditor]
  public AimAssistQuery AimAssistQuery;
  KCharacterController CharacterController;
  LocalClock LocalClock;

  public override void OnStart(GameObject runner, GameObject owner) {
    CharacterController = owner.GetComponent<KCharacterController>();
    LocalClock = owner.GetComponent<LocalClock>();
  }

  public override void OnUpdate(GameObject runner, GameObject owner) {
    var bestTarget = AimAssistManager.Instance.BestTarget(owner.transform, AimAssistQuery);
    if (bestTarget) {
      var direction = bestTarget.transform.position-owner.transform.position;
      var maxDegrees = TurnSpeed * LocalClock.DeltaTime();
      var nextRotation = Quaternion.RotateTowards(
        CharacterController.transform.rotation,
        Quaternion.LookRotation(direction),
        maxDegrees);
      CharacterController.Rotation.Set(nextRotation);
    }
  }
}

[Serializable]
public class WeaponAimBehavior : FrameBehavior {
  public Vector3 Direction;

  WeaponAim WeaponAim;

  public override void OnStart(GameObject runner, GameObject owner) {
    WeaponAim = owner.GetComponent<AbilityManager>().LocateComponent<WeaponAim>();
    WeaponAim.AimDirection = Direction;
  }

  public override void OnEnd(GameObject runner, GameObject owner) {
    WeaponAim.AimDirection = null;
  }
}

[Serializable]
public class HitboxBehavior : FrameBehavior {
}

[Serializable]
public class AudioOneShotBehavior : FrameBehavior {
  public float Volume = 0.25f;
  public AudioClip AudioClip;

  public override void OnStart(GameObject runner, GameObject owner) {
    var audioSource = new GameObject($"AudioSourceOneShot({AudioClip.name})").AddComponent<AudioSource>();
    audioSource.spatialBlend = 0;
    audioSource.clip = AudioClip;
    audioSource.volume = Volume;
    audioSource.transform.position = owner.transform.position;
    audioSource.Play();
    GameObject.Destroy(audioSource.gameObject, AudioClip.length);
  }
}

[Serializable]
public class AnimatorControllerCrossFadeBehavior : FrameBehavior {
  public string StartStateName;
  public string EndStateName;
  public int LayerIndex;
  public float CrossFadeDuration = 0.25f;
  public override string Name => string.IsNullOrEmpty(StartStateName) ? base.Name : $"AnimatorState({StartStateName})";

  AnimatorCallbackHandler AnimatorCallbackHandler;

  public override void OnStart(GameObject runner, GameObject owner) {
    AnimatorCallbackHandler = owner.GetComponent<AbilityManager>().LocateComponent<AnimatorCallbackHandler>();
    AnimatorCallbackHandler.Animator.CrossFadeInFixedTime(StartStateName, CrossFadeDuration, LayerIndex);
  }

  public override void OnEnd(GameObject runner, GameObject owner) {
    AnimatorCallbackHandler.Animator.Play(EndStateName, LayerIndex);
  }
}

[Serializable]
public class VisualEffectBehavior : FrameBehavior {
  const float MAX_VFX_LIFETIME = 10;

  public VisualEffectAsset VisualEffectAsset;
  public string StartEventName = "OnPlay";
  public string UpdateEventName = "";
  public string EndEventName = "";
  public bool AttachedToOwner;
  public Vector3 Offset;
  public Vector3 Rotation;

  VisualEffect VisualEffect;

  public override void OnStart(GameObject runner, GameObject owner) {
    if (VisualEffectAsset) {
      VisualEffect = new GameObject().AddComponent<VisualEffect>();
      VisualEffect.gameObject.name = $"Instance({VisualEffectAsset.name})";
      VisualEffect.visualEffectAsset = VisualEffectAsset;
      VisualEffect.initialEventName = "";
      VisualEffect.PlayNonEmptyEvent(StartEventName);
      VisualEffect.transform.SetParent(AttachedToOwner ? owner.transform : null);
      VisualEffect.transform.SetLocalPositionAndRotation(Offset, Quaternion.Euler(Rotation));
    }
  }

  public override void OnUpdate(GameObject runner, GameObject owner) {
    if (VisualEffect) {
      VisualEffect.PlayNonEmptyEvent(UpdateEventName);
    }
  }

  public override void OnEnd(GameObject runner, GameObject owner) {
    if (VisualEffect) {
      VisualEffect.PlayNonEmptyEvent(EndEventName);
      GameObject.Destroy(VisualEffect.gameObject, MAX_VFX_LIFETIME);
    }
  }
}

[Serializable]
public class CancelBehavior : FrameBehavior {
}

public class AttackAbility : Ability {
  [SerializeField] RootMotionBehavior RootMotionBehavior;
  [SerializeField] AimAssistBehavior AimAssistBehavior;
  [SerializeField] WeaponAimBehavior WeaponAimBehavior;
  [SerializeField] HitboxBehavior HitboxBehavior;
  [SerializeField] AudioOneShotBehavior AudioOneShotBehavior;
  [SerializeField] AnimatorControllerCrossFadeBehavior CrossFadeStateBehavior;
  [SerializeField] VisualEffectBehavior VisualEffectBehavior;
  [SerializeField] CancelBehavior CancelBehavior;
  [SerializeField, Min(0)] int EndFrame = 24;
  [SerializeField, Min(0)] int Frame;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;

  bool Cancellable;
  List<Combatant> Struck = new(16);

  IEnumerable<FrameBehavior> Behaviors {
    get {
      yield return RootMotionBehavior;
      yield return AimAssistBehavior;
      yield return WeaponAimBehavior;
      yield return HitboxBehavior;
      yield return AudioOneShotBehavior;
      yield return CrossFadeStateBehavior;
      yield return VisualEffectBehavior;
      yield return CancelBehavior;
    }
  }

  void Awake() {
    Frame = EndFrame+1;
    Hitbox.CollisionEnabled = false;
    Struck.Clear();
  }

  public bool ShouldHit(Combatant combatant) => !Struck.Contains(combatant);

  public void Hit(Combatant combatant) => Struck.Add(combatant);

  public override bool IsRunning => Frame <= EndFrame;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    Frame = 0;
  }

  public bool CanAim => Frame == 0;
  public void Aim(Vector2 direction) {
    if (direction.sqrMagnitude > 0)
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ()));
  }

  public override bool CanCancel => Cancellable;
  public override void Cancel() {
    CancelActiveBehaviors();
    Frame = EndFrame+1;
  }

  void FixedUpdate() {
    if (IsRunning) {
      StartBehaviors();
      UpdateBehaviors();
      EndBehaviors();
      Frame++;
    }
  }

  void StartBehaviors() {
    foreach (var behavior in Behaviors) {
      if (behavior.Starting(Frame)) {
        behavior.OnStart(gameObject, AbilityManager.gameObject);
      }
    }
    if (HitboxBehavior.Starting(Frame)) {
      Hitbox.CollisionEnabled = true;
      Struck.Clear();
    }
    if (CancelBehavior.Starting(Frame))
      Cancellable = true;
  }

  void EndBehaviors() {
    foreach (var behavior in Behaviors) {
      if (behavior.Ending(Frame)) {
        behavior.OnEnd(gameObject, AbilityManager.gameObject);
      }
    }
    if (HitboxBehavior.Ending(Frame)) {
      Hitbox.CollisionEnabled = false;
      Struck.Clear();
    }
    if (CancelBehavior.Ending(Frame))
      Cancellable = false;
  }

  void UpdateBehaviors() {
    foreach (var behavior in Behaviors) {
      if (behavior.Active(Frame)) {
        behavior.OnUpdate(gameObject, AbilityManager.gameObject);
      }
    }
    if (HitboxBehavior.Active(Frame)) {}
    if (CancelBehavior.Active(Frame)) {}
  }

  void CancelActiveBehaviors() {
    foreach (var behavior in Behaviors) {
      if (behavior.Active(Frame)) {
        behavior.OnEnd(gameObject, AbilityManager.gameObject);
      }
    }
    if (HitboxBehavior.Active(Frame)) {
      Struck.Clear();
      Hitbox.CollisionEnabled = false;
    }
    if (CancelBehavior.Active(Frame)) {
      Cancellable = false;
    }
  }
}