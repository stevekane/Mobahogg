#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

/*
NOTE!
This should absolutely not exist in the Engine assembly but only in the editor assembly.
There is some preview code currently that makes use of this which has not been isolated to
the Editor assembly as it's all still in flux or something.
*/

[InitializeOnLoad]
public static class EditorAudioSystem {
  static MethodInfo PlayMethod;
  static MethodInfo StopAllMethod;
  static MethodInfo GetPreviewClipInstanceMethod;
  static MethodInfo SetPreviewClipVolumeMethod; // if available

  static Type audioUtilClass;

  static EditorAudioSystem() {
    Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
    audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
    PlayMethod = audioUtilClass.GetMethod(
      "PlayPreviewClip",
      BindingFlags.Static | BindingFlags.Public,
      null,
      new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
      null);
    StopAllMethod = audioUtilClass.GetMethod(
      "StopAllPreviewClips",
      BindingFlags.Static | BindingFlags.Public,
      null,
      Type.EmptyTypes,
      null);
    GetPreviewClipInstanceMethod = audioUtilClass.GetMethod(
      "GetPreviewClipInstance",
      BindingFlags.Static | BindingFlags.Public,
      null,
      new Type[] { typeof(AudioClip) },
      null);
    // Some Unity versions expose a SetPreviewClipVolume method:
    SetPreviewClipVolumeMethod = audioUtilClass.GetMethod(
      "SetPreviewClipVolume",
      BindingFlags.Static | BindingFlags.Public,
      null,
      new Type[] { typeof(AudioClip), typeof(float) },
      null);
  }

  public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false) {
    if(clip == null) return;
    PlayMethod.Invoke(null, new object[] { clip, startSample, loop });
  }

  public static void StopAllClips() {
    StopAllMethod.Invoke(null, null);
  }

  // Option 1: Use a direct internal method to set volume (if available)
  public static void SetClipVolume_Internal(AudioClip clip, float volume) {
    if(clip == null) return;
    if (SetPreviewClipVolumeMethod != null) {
      SetPreviewClipVolumeMethod.Invoke(null, new object[] { clip, volume });
    }
    else {
      // Option 2: Retrieve the preview instance and set its volume manually.
      AudioSource src = GetPreviewClipInstance(clip);
      if(src != null)
        src.volume = volume;
    }
  }

  // Helper: Retrieve the preview AudioSource instance for the clip.
  public static AudioSource GetPreviewClipInstance(AudioClip clip) {
    if(clip == null || GetPreviewClipInstanceMethod == null)
      return null;
    return GetPreviewClipInstanceMethod.Invoke(null, new object[] { clip }) as AudioSource;
  }
}

#endif