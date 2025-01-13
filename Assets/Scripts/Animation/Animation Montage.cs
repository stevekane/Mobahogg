using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationMontage", menuName = "Animation/Montage")]
public class AnimationMontage : ScriptableObject {
  public List<AnimationMontageClip> Clips = new();
  public List<AnimationNotify> Notifies = new();
}

[Serializable]
public class AnimationMontageClip {
  public AnimationClip AnimationClip;
  public int StartFrame;
  public int Duration => Mathf.RoundToInt(AnimationClip.length * 60); // Derived from clip length at 60fps
  public int EndFrame => StartFrame + Duration;
}

[Serializable]
public class AnimationNotify {
  public string Name;
  public int StartFrame = 0;
  public int EndFrame = 1;
  public int FrameDuration => EndFrame-StartFrame;
}
