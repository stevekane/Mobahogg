using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AnimationMontage", menuName = "Animation/Montage")]
public class AnimationMontage : PlayableAsset {
  #if UNITY_EDITOR
  public Animator AnimatorPrefab;
  #endif
  public List<AnimationMontageClip> Clips = new();
  public List<AnimationNotify> Notifies = new();
  public int FrameDuration =>
    Mathf.Max(Clips.Max(c => c.EndFrame), Notifies.Max(n => n.EndFrame));

  public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
    return CreateScriptPlayable(graph);
  }

  public ScriptPlayable<AnimationMontagePlayableBehavior> CreateScriptPlayable(PlayableGraph graph) {
    var playable = ScriptPlayable<AnimationMontagePlayableBehavior>.Create(graph);
    var behavior = playable.GetBehaviour();
    playable.SetTime(0);
    playable.SetDuration(FrameDuration / 60f);
    Clips.ForEach(behavior.Add);
    Notifies.ForEach(behavior.Add);
    return playable;
  }
}

[Serializable]
public class AnimationMontageClip {
  public string Name = "Animation Montage Clip";
  public AnimationClip AnimationClip;
  public bool FootIK;
  public int StartFrame;
  public float Speed = 1;
  public int Duration => AnimationClip ? Mathf.RoundToInt(AnimationClip.length / Speed * 60) : 0;
  public int EndFrame => StartFrame + Duration;
  public int FadeInFrames;
  public int FadeOutFrames;

  public float Weight(int frame) {
    if (FadeInFrames > 0 && frame >= StartFrame && frame <= StartFrame+FadeInFrames) {
      return Mathf.InverseLerp(StartFrame, StartFrame+FadeInFrames, frame);
    } else if (FadeOutFrames > 0 && frame >= EndFrame-FadeOutFrames && frame <= EndFrame) {
      return 1-Mathf.InverseLerp(EndFrame-FadeOutFrames, EndFrame, frame);
    } else if (frame >= StartFrame && frame <= EndFrame) {
      return 1;
    } else {
      return 0;
    }
  }
}

[Serializable]
public class AnimationNotify {
  public string Name;
  public int StartFrame = 0;
  public int EndFrame = 1;
  public int FrameDuration => EndFrame-StartFrame;
}

public class AnimationMontagePlayableBehavior : PlayableBehaviour {
  AnimationMixerPlayable mixerPlayable;
  PlayableGraph playableGraph;
  List<AnimationClipPlayable> clipPlayables = new List<AnimationClipPlayable>();
  List<AnimationMontageClip> montageClips = new List<AnimationMontageClip>();
  List<AnimationNotify> notifies = new List<AnimationNotify>();

  public void Add(AnimationMontageClip montageClip) {
    var clipPlayable = AnimationClipPlayable.Create(playableGraph, montageClip.AnimationClip);
    clipPlayable.SetSpeed(montageClip.Speed);
    clipPlayable.SetApplyFootIK(montageClip.FootIK);
    clipPlayable.SetApplyPlayableIK(montageClip.FootIK);
    clipPlayable.SetTime(0);
    clipPlayable.SetDuration(montageClip.Duration / 60f);
    mixerPlayable.AddInput(clipPlayable, 0, 0f);
    clipPlayables.Add(clipPlayable);
    montageClips.Add(montageClip);
  }

  public void Add(AnimationNotify notify) {
    notifies.Add(notify);
  }

  public override void OnPlayableCreate(Playable playable) {
    playableGraph = playable.GetGraph();
    mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 0);
    playable.AddInput(mixerPlayable, 0, 1);
  }

  public override void OnPlayableDestroy(Playable playable) {
    if (playableGraph.IsValid() && mixerPlayable.IsValid()) {
      playableGraph.DestroySubgraph(mixerPlayable);
    }
  }

  public override void PrepareFrame(Playable playable, FrameData info) {
    var frame = Mathf.RoundToInt((float)playable.GetTime() * 60);
    var clipCount = clipPlayables.Count;
    for (var i = 0; i < clipCount; i++) {
      var clipPlayable = clipPlayables[i];
      var montageClip = montageClips[i];
      var interpolant = Mathf.InverseLerp(montageClip.StartFrame, montageClip.EndFrame, frame);
      clipPlayable.SetTime(interpolant * montageClip.Speed * clipPlayable.GetDuration());
      mixerPlayable.SetInputWeight(i, montageClip.Weight(frame));
    }
  }

  public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
    if (playerData is MonoBehaviour) {
      var go = (MonoBehaviour)playerData;
      var frame = Mathf.RoundToInt((float)playable.GetTime() * 60);
      foreach (var notify in notifies) {
        if (notify.StartFrame == frame) {
          go.SendMessage("OnNotifyStart", notify, SendMessageOptions.DontRequireReceiver);
        }
        if (notify.EndFrame == frame) {
          go.SendMessage("OnNotifyEnd", notify, SendMessageOptions.DontRequireReceiver);
        }
      }
    }
    if (playerData is GameObject) {
      var go = (GameObject)playerData;
      var frame = Mathf.RoundToInt((float)playable.GetTime() * 60);
      foreach (var notify in notifies) {
        if (notify.StartFrame == frame) {
          go.SendMessage("OnNotifyStart", notify, SendMessageOptions.DontRequireReceiver);
        }
        if (notify.EndFrame == frame) {
          go.SendMessage("OnNotifyEnd", notify, SendMessageOptions.DontRequireReceiver);
        }
      }
    }
  }
}