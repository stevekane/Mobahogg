using System;
using System.ComponentModel;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public partial class SFXOneShotFrameBehavior {
  public override void PreviewOnStart(PreviewRenderUtility preview) {
    // EditorAudioSystem.PlayClip(AudioClip, 0, false);
  }
}
#endif

[Serializable]
[DisplayName("SFX One Shot")]
public partial class SFXOneShotFrameBehavior : FrameBehavior {
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
}
