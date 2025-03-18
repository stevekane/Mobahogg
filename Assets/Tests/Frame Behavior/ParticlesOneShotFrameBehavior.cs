using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;

public partial class ParticlesOneShotFrameBehavior {
  public override void PreviewInitialize(object provider) {
    TryGetValue(provider, BehaviorTag, out Parent);
  }

  public override void PreviewCleanup(object provider) {
    ParticleSystem.TryDestroyImmediateGameObject();
  }

  public override void PreviewOnStart(PreviewRenderUtility preview) {
    if (ParticleSystemPrefab) {
      ParticleSystem = GameObject.Instantiate(ParticleSystemPrefab);
      SceneManager.MoveGameObjectToScene(ParticleSystem.gameObject, Parent.gameObject.scene);
      ParticleSystem.transform.position = Parent.transform.position + Parent.TransformVector(Offset);
      ParticleSystem.transform.SetParent(AttachedToParent ? Parent : null);
      // This order matters. seed cannot be set while running and run on awake is common
      ParticleSystem.Stop();
      ParticleSystem.randomSeed = 0;
      ParticleSystem.Play();
      ParticleSystem.Pause();
    }
  }

  public override void PreviewOnLateUpdate(PreviewRenderUtility preview) {
    if (!ParticleSystem)
      return;
    ParticleSystem.Simulate(Time.fixedDeltaTime, withChildren: true, restart: false);
  }
}
#endif

[Serializable]
[DisplayName("Particles OneShot")]
public partial class ParticlesOneShotFrameBehavior : FrameBehavior {
  public ParticleSystem ParticleSystemPrefab;
  public BehaviorTag BehaviorTag;
  public bool AttachedToParent;
  public Vector3 Offset;

  Transform Parent;
  ParticleSystem ParticleSystem;

  public override void Initialize(object provider) {
    TryGetValue(provider, BehaviorTag, out Parent);
  }

  public override void Cleanup(object provider) {
    ParticleSystem.DestroyGameObject();
  }

  public override void OnStart() {
    if (ParticleSystemPrefab) {
      ParticleSystem = GameObject.Instantiate(ParticleSystemPrefab);
      ParticleSystem.transform.position = Parent.transform.position + Parent.TransformVector(Offset);
      ParticleSystem.transform.SetParent(AttachedToParent ? Parent : null);
    }
  }
}