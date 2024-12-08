using System;
using System.Collections.Generic;
using UnityEngine;

public static class AudioExtensions {
  public static void Play(this AudioSource source, AudioClip clip) {
    source.Stop();
    source.clip = clip;
    source.Play();
  }
}

public class AudioManager : SingletonBehavior<AudioManager> {
  [SerializeField] AudioClip BackgroundMusic;
  [SerializeField] float SoundCooldown = 0.05f;
  [SerializeField] AudioSource MusicSource;
  [SerializeField] AudioSource SoundSource;

  Dictionary<AudioClip, float> SoundLastPlayed = new();

  public void ResetBackgroundMusic() {
    MusicSource.Play(BackgroundMusic);
  }

  public void PlaySoundWithCooldown(AudioClip clip) {
    if (!clip) return;
    var lastPlayed = SoundLastPlayed.GetValueOrDefault(clip);
    if (Time.time < lastPlayed + SoundCooldown)
      return;
    SoundLastPlayed[clip] = Time.time;
    SoundSource.PlayOneShot(clip);
  }
}