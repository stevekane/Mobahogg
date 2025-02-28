#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

/*
You could probably cache these after Assemblies load or something to avoid doing all this
reflection garbage all the time.

NOTE!
This should absolutely not exist in the Engine assembly but only in the editor assembly.
There is some preview code currently that makes use of this which has not been isolated to
the Editor assembly as it's all still in flux or something.
*/
public static class EditorAudioSystem {
  public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false) {
    Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
    Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
    MethodInfo method = audioUtilClass.GetMethod(
      "PlayPreviewClip",
      BindingFlags.Static | BindingFlags.Public,
      null,
      new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
      null);
    method.Invoke(
      null,
      new object[] { clip, startSample, loop });
  }

  public static void StopAllClips() {
    Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
    Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
    MethodInfo method = audioUtilClass.GetMethod(
      "StopAllPreviewClips",
      BindingFlags.Static | BindingFlags.Public,
      null,
      new Type[] { },
      null);
    method.Invoke(
      null,
      new object[] { });
  }
}
#endif