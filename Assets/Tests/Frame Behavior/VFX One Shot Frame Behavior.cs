using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[DisplayName("VFX One Shot")]
public class VFXOneShotFrameBehavior : FrameBehavior {
  const float MAX_VFX_LIFETIME = 10;

  public VisualEffectAsset VisualEffectAsset;
  public string StartEventName = "OnPlay";
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
    var name = VisualEffectAsset ? VisualEffectAsset.name : "EMPTY_VFX";
    VisualEffect = new GameObject($"Instance({name})").AddComponent<VisualEffect>();
    SceneManager.MoveGameObjectToScene(VisualEffect.gameObject, Owner.gameObject.scene);
    VisualEffect.gameObject.name = $"Instance({VisualEffectAsset.name})";
    VisualEffect.visualEffectAsset = VisualEffectAsset;
    VisualEffect.initialEventName = StartEventName;
    VisualEffect.resetSeedOnPlay = false;
    VisualEffect.transform.SetParent(AttachedToOwner ? Owner.transform : null);
    VisualEffect.transform.SetLocalPositionAndRotation(Offset, Quaternion.Euler(Rotation));
    VisualEffect.transform.localScale = Scale;
    VisualEffect.gameObject.AddComponent<Shortlived>().Lifetime = Timeval.FromSeconds(MAX_VFX_LIFETIME);
  }

  #if UNITY_EDITOR
  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, null, out Owner);
  }

  public override void PreviewCleanup(object provider) {
    VisualEffect.TryDestroyImmediateGameObject();
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    var name = VisualEffectAsset ? VisualEffectAsset.name : "EMPTY_VFX";
    VisualEffect = new GameObject($"Instance({name})").AddComponent<VisualEffect>();
    SceneManager.MoveGameObjectToScene(VisualEffect.gameObject, Owner.gameObject.scene);
    VisualEffect.visualEffectAsset = VisualEffectAsset;
    VisualEffect.initialEventName = StartEventName;
    VisualEffect.resetSeedOnPlay = false;
    VisualEffect.transform.SetParent(AttachedToOwner ? Owner.transform : null);
    VisualEffect.transform.SetLocalPositionAndRotation(Offset, Quaternion.Euler(Rotation));
    VisualEffect.transform.localScale = Scale;
    VisualEffect.pause = true;
    VisualEffect.Reinit();
  }

  public override void PreviewOnUpdate(PreviewRenderUtility preview) {
    VisualEffect.Simulate(Time.fixedDeltaTime);
  }
#endif
}