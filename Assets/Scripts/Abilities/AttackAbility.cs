using System.Collections.Generic;
using UnityEngine;
using AimAssist;
using Abilities;
using System;
using UnityEngine.VFX;
using UnityEngine.Playables;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class RootMotionBehavior : FrameBehavior {
  public float Multiplier = 1;
  AnimatorCallbackHandler AnimatorCallbackHandler;
  KCharacterController CharacterController;
  LocalClock LocalClock;

  void OnRootMotion() {
    var forward = CharacterController.Rotation.Forward;
    var dp = Vector3.Project(AnimatorCallbackHandler.Animator.deltaPosition, forward);
    var v = dp / LocalClock.DeltaTime();
    CharacterController.DirectVelocity.Add(Multiplier * v);
  }

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out AnimatorCallbackHandler);
    TryGetValue(provider, null, out CharacterController);
    TryGetValue(provider, null, out LocalClock);
  }

  public override void OnStart() {
    AnimatorCallbackHandler.OnRootMotion.Listen(OnRootMotion);
  }

  public override void OnEnd() {
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

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out CharacterController);
    TryGetValue(provider, null, out LocalClock);
  }

  public override void OnUpdate() {
    var bestTarget = AimAssistManager.Instance.BestTarget(CharacterController.transform, AimAssistQuery);
    if (bestTarget) {
      var direction = bestTarget.transform.position-CharacterController.transform.position;
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

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out WeaponAim);
  }

  public override void OnStart() {
    WeaponAim.AimDirection = Direction;
  }

  public override void OnEnd() {
    WeaponAim.AimDirection = null;
  }
}

[Serializable]
public class HitboxBehavior : FrameBehavior {
  Hitbox Hitbox;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Hitbox);
  }

  public override void OnStart() {
    Hitbox.CollisionEnabled = true;
  }

  public override void OnEnd() {
    Hitbox.CollisionEnabled = false;
  }
}

[Serializable]
public class SFXOneShotBehavior : FrameBehavior {
  public float Volume = 0.25f;
  public AudioClip AudioClip;

  GameObject Owner;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Owner);
  }

  public override void OnStart() {
    var audioSource = new GameObject($"AudioSourceOneShot({AudioClip.name})").AddComponent<AudioSource>();
    audioSource.spatialBlend = 0;
    audioSource.clip = AudioClip;
    audioSource.volume = Volume;
    audioSource.transform.position = Owner.transform.position;
    audioSource.Play();
    GameObject.Destroy(audioSource.gameObject, AudioClip.length);
  }

#if UNITY_EDITOR
  public override void PreviewOnStart(PreviewRenderUtility preview) {
    // EditorAudioSystem.PlayClip(AudioClip, 0, false);
  }
#endif
}

[Serializable]
public class AnimationOneShot : FrameBehavior {
  public string StartStateName;
  public string EndStateName;
  public int LayerIndex;
  public float CrossFadeDuration = 0.1f;

  public override FrameBehavior Clone() {
    return new AnimationOneShot {
      StartFrame = StartFrame,
      EndFrame = EndFrame,
      StartStateName = StartStateName,
      EndStateName = EndStateName,
      LayerIndex = LayerIndex,
      CrossFadeDuration = CrossFadeDuration
    };
  }

  Animator Animator;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Animator);
  }

  public override void OnStart() {
    Animator.CrossFadeInFixedTime(StartStateName, CrossFadeDuration, LayerIndex);
  }

  public override void OnEnd() {
    Animator.Play(EndStateName, LayerIndex);
  }

  #if UNITY_EDITOR
  AnimationClip AnimationClip;
  float Speed;
  PlayableGraph PlayableGraph;
  AnimationClipPlayable AnimationClipPlayable;

  ~AnimationOneShot() {
    if (PlayableGraph.IsValid()) {
      PlayableGraph.Destroy();
    }
  }

  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, null, out Animator);
    if (!Animator)
      return;
    var state = Animator.StateForNameInLayer(StartStateName, LayerIndex);
    if (!state)
      return;
    AnimationClip = state.motion as AnimationClip;
    Speed = state.speed;
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    if (Animator && AnimationClip) {
      PlayableGraph = PlayableGraph.Create("AnimationOneShotPreview");
      PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
      AnimationClipPlayable = AnimationClipPlayable.Create(PlayableGraph, AnimationClip);
      AnimationClipPlayable.SetSpeed(Speed);
      var output = AnimationPlayableOutput.Create(PlayableGraph, "Animation Output", Animator);
      output.SetSourcePlayable(AnimationClipPlayable, 0);
      PlayableGraph.Play();
      PlayableGraph.Evaluate();
    }
  }

  public override void PreviewOnEnd(PreviewRenderUtility preview) {
    if (PlayableGraph.IsValid()) {
      PlayableGraph.Destroy();
    }
  }

  public override void PreviewOnUpdate(PreviewRenderUtility preview) {
    if (PlayableGraph.IsValid()) {
      PlayableGraph.Evaluate(1/60f);
    }
  }
  #endif
}

[Serializable]
public class VFXOneShot : FrameBehavior {
  const float MAX_VFX_LIFETIME = 10;

  public VisualEffectAsset VisualEffectAsset;
  public string StartEventName = "OnPlay";
  public string UpdateEventName = "";
  public string EndEventName = "";
  public bool AttachedToOwner;
  public Vector3 Offset;
  public Vector3 Rotation;
  public Vector3 Scale = Vector3.one;

  GameObject Owner;

  VisualEffect VisualEffect;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Owner);
  }

  public override void OnStart() {
    if (VisualEffectAsset) {
      VisualEffect = new GameObject().AddComponent<VisualEffect>();
      VisualEffect.gameObject.name = $"Instance({VisualEffectAsset.name})";
      VisualEffect.visualEffectAsset = VisualEffectAsset;
      VisualEffect.initialEventName = "";
      VisualEffect.PlayNonEmptyEvent(StartEventName);
      VisualEffect.transform.SetParent(AttachedToOwner ? Owner.transform : null);
      VisualEffect.transform.SetLocalPositionAndRotation(Offset, Quaternion.Euler(Rotation));
      VisualEffect.transform.localScale = Scale;
    }
  }

  public override void OnUpdate() {
    if (VisualEffect) {
      VisualEffect.PlayNonEmptyEvent(UpdateEventName);
    }
  }



  public override void OnEnd() {
    if (VisualEffect) {
      VisualEffect.PlayNonEmptyEvent(EndEventName);
      GameObject.Destroy(VisualEffect.gameObject, MAX_VFX_LIFETIME);
    }
  }

  #if UNITY_EDITOR
  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, null, out Owner);
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    if (VisualEffectAsset) {
      VisualEffect = new GameObject().AddComponent<VisualEffect>();
      preview.AddSingleGO(VisualEffect.gameObject);
      VisualEffect.gameObject.name = $"Instance({VisualEffectAsset.name})";
      VisualEffect.visualEffectAsset = VisualEffectAsset;
      VisualEffect.initialEventName = "";
      VisualEffect.PlayNonEmptyEvent(StartEventName);
      VisualEffect.transform.SetParent(AttachedToOwner ? Owner.transform : null);
      VisualEffect.transform.SetLocalPositionAndRotation(Offset, Quaternion.Euler(Rotation));
      VisualEffect.transform.localScale = Scale;
    }
  }

  public override void PreviewOnUpdate(PreviewRenderUtility preview) {
    if (VisualEffect) {
      VisualEffect.PlayNonEmptyEvent(UpdateEventName);
    }
  }

  public override void PreviewOnEnd(PreviewRenderUtility preview) {
    if (VisualEffect) {
      VisualEffect.PlayNonEmptyEvent(EndEventName);
    }
  }

  #endif
}

[Serializable]
public class CancelBehavior : FrameBehavior {
  ICancellable Cancellable;

  public override void Initialize(object provider) {
    TryGetValue(provider, null, out Cancellable);
  }

  public override void OnStart() {
    Cancellable.Cancellable = true;
  }

  public override void OnEnd() {
    Cancellable.Cancellable = false;
  }
}

// Steve - this is kind of a temporary sort of thing until I think of something
// less bespoke to do. Maybe cancellation by tags, etc. Here to keep things simple
public interface ICancellable {
  public bool Cancellable { get; set; }
}

public class AttackAbility :
  Ability,
  ICancellable,
  IProvider<Animator>,
  IProvider<AnimatorCallbackHandler>,
  IProvider<KCharacterController>,
  IProvider<WeaponAim>,
  IProvider<Hitbox>,
  IProvider<GameObject>,
  IProvider<LocalClock>,
  IProvider<ICancellable>
{
  [SerializeField] RootMotionBehavior RootMotionBehavior;
  [SerializeField] AimAssistBehavior AimAssistBehavior;
  [SerializeField] WeaponAimBehavior WeaponAimBehavior;
  [SerializeField] HitboxBehavior HitboxBehavior;
  [SerializeField] SFXOneShotBehavior AudioOneShotBehavior;
  [SerializeField] AnimationOneShot CrossFadeStateBehavior;
  [SerializeField] VFXOneShot VisualEffectBehavior;
  [SerializeField] CancelBehavior CancelBehavior;
  [SerializeField, Min(0)] int EndFrame = 24;
  [SerializeField, Min(0)] int Frame;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;

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
  }

  // Steve - It is worth contemplating whether most or all characters might have a single
  // top-level provider that implements this shit. If so, you could move this noise out of here
  // and create a very elegant Ability / User of FrameBehaviors
  AnimatorCallbackHandler IProvider<AnimatorCallbackHandler>.Value(BehaviorTag tag) => AnimatorCallbackHandler;
  Animator IProvider<Animator>.Value(BehaviorTag tag) => AnimatorCallbackHandler.Animator;
  KCharacterController IProvider<KCharacterController>.Value(BehaviorTag tag) => CharacterController;
  WeaponAim IProvider<WeaponAim>.Value(BehaviorTag tag) => AbilityManager.LocateComponent<WeaponAim>();
  GameObject IProvider<GameObject>.Value(BehaviorTag tag) => AbilityManager.gameObject;
  LocalClock IProvider<LocalClock>.Value(BehaviorTag tag) => LocalClock;
  Hitbox IProvider<Hitbox>.Value(BehaviorTag tag) => Hitbox;
  ICancellable IProvider<ICancellable>.Value(BehaviorTag tag) => this;

  public bool Cancellable { get; set; }
  public override bool CanCancel => Cancellable;
  public override void Cancel() {
    FrameBehavior.CancelActiveBehaviors(Behaviors, Frame);
    Frame = EndFrame+1;
  }

  public override bool IsRunning => Frame <= EndFrame;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    Frame = 0;
    FrameBehavior.InitializeBehaviors(Behaviors, this);
  }

  public bool CanAim => Frame == 0;
  public void Aim(Vector2 direction) {
    if (direction.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ()));
    }
  }

  void FixedUpdate() {
    if (IsRunning) {
      FrameBehavior.StartBehaviors(Behaviors, Frame);
      FrameBehavior.UpdateBehaviors(Behaviors, Frame);
      FrameBehavior.EndBehaviors(Behaviors, Frame);
      Frame++;
    }
  }
}