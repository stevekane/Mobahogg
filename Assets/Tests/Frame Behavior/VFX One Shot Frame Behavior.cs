using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;

public partial class VFXOneShotFrameBehavior {
  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, BehaviorTag, out Parent);
  }

  public override void PreviewCleanup(object provider) {
    VisualEffect.TryDestroyImmediateGameObject();
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    if (VisualEffectPrefab) {
      VisualEffect = GameObject.Instantiate(VisualEffectPrefab);
      SceneManager.MoveGameObjectToScene(VisualEffect.gameObject, Parent.gameObject.scene);
      VisualEffect.transform.position = Parent.transform.position + Parent.TransformVector(Offset);
      VisualEffect.transform.SetParent(AttachedToParent ? Parent : null);

      // Specific to preview setup
      VisualEffect.resetSeedOnPlay = false;
      VisualEffect.pause = true;
      VisualEffect.Reinit();
    }
  }

  public override void PreviewOnUpdate(PreviewRenderUtility preview) {
    if (!VisualEffect)
      return;
    VisualEffect.Simulate(Time.fixedDeltaTime);
  }
}
#endif

[Serializable]
[DisplayName("VFX One Shot")]
public partial class VFXOneShotFrameBehavior : FrameBehavior {
  const float MAX_VFX_LIFETIME = 10;

  public VisualEffect VisualEffectPrefab;
  public string StartEventName = "OnPlay";
  public BehaviorTag BehaviorTag;
  public bool AttachedToParent;
  public Vector3 Offset;

  Transform Parent;
  VisualEffect VisualEffect;

  public override void Initialize(object provider) {
    TryGetValue(provider, BehaviorTag, out Parent);
  }

  public override void OnStart() {
    if (VisualEffectPrefab) {
      VisualEffect = GameObject.Instantiate(VisualEffectPrefab);
      SceneManager.MoveGameObjectToScene(VisualEffect.gameObject, Parent.gameObject.scene);
      VisualEffect.transform.position = Parent.transform.position + Parent.TransformVector(Offset);
      VisualEffect.transform.SetParent(AttachedToParent ? Parent : null);
      VisualEffect.gameObject.AddComponent<Shortlived>().Lifetime = Timeval.FromSeconds(MAX_VFX_LIFETIME);
    }
  }
}