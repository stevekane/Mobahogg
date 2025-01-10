using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine;

namespace Animation {
  struct SlotClipConfig {
    public double FadeOutStart;
    public double FadeOutEnd;
  }

  public class SlotBehavior : PlayableBehaviour {
    public bool IsRunning => ActiveIndex > 0;
    public Playable ActivePlayable => Mixer.GetInput(ActiveIndex);
    public double FadeDuration = 0.25f;

    Playable Playable;
    AnimationMixerPlayable Mixer;
    SlotClipConfig[] SlotClipConfigs = new SlotClipConfig[32];
    int ActiveIndex;

    int? OpenPort {
      get {
        var portCount = Mixer.GetInputCount();
        for (var i = 1; i < portCount; i++) {
          var input = Mixer.GetInput(i);
          if (input.IsNull() || !input.IsValid()) {
            return i;
          }
        }
        return null;
      }
    }

    public void Connect(Playable playable) {
      Mixer.ConnectInput(0, playable, 0, 1);
    }

    public void Play(AnimationClipPlayable playable) {
      if (IsRunning) {
        SlotClipConfigs[ActiveIndex].FadeOutStart = Playable.GetTime();
        SlotClipConfigs[ActiveIndex].FadeOutEnd = Playable.GetTime() + FadeDuration;
      }
      var openPort = OpenPort;
      if (openPort.HasValue) {
        Mixer.ConnectInput(openPort.Value, playable, 0, 0);
        ActiveIndex = openPort.Value;
      } else {
        ActiveIndex = Mixer.GetInputCount();
        Mixer.AddInput(playable, 0, 0);
      }
      SlotClipConfigs[ActiveIndex].FadeOutStart = playable.GetDuration()-FadeDuration;
      SlotClipConfigs[ActiveIndex].FadeOutEnd = playable.GetDuration();
    }

    // TODO: Add Stop which is sort of like playing nothing new
    public override void OnPlayableCreate(Playable playable) {
      Playable = playable;
      Mixer = AnimationMixerPlayable.Create(playable.GetGraph(), 1);
      playable.ConnectInput(0, Mixer, 0, 1);
    }

    public override void OnPlayableDestroy(Playable playable) {
      playable.GetGraph().DestroySubgraph(Mixer);
    }

    public override void PrepareFrame(Playable playable, FrameData info) {
      var count = Mixer.GetInputCount();

      // Check if we have reached fade start for the current active playable
      if (IsRunning) {
        var activePlayable = Mixer.GetInput(ActiveIndex);
        var activeSlotClipConfig = SlotClipConfigs[ActiveIndex];
        if (!activePlayable.IsNull() && activePlayable.GetTime() >= activeSlotClipConfig.FadeOutStart) {
          ActiveIndex = 0;
        }
      }

      // Update fade values
      for (var i = 0; i < count; i++) {
        var currentWeight = Mixer.GetInputWeight(i);
        var targetWeight = i == ActiveIndex ? 1 : 0;
        var nextWeight = Mathf.MoveTowards(currentWeight, targetWeight, info.deltaTime / (float)FadeDuration);
        Mixer.SetInputWeight(i, nextWeight);
      }

      // Remove any completed clips
      for (var i = count-1; i > 0; i--) {
        var input = Mixer.GetInput(i);
        if (input.IsNull() || !input.IsValid())
          continue;
        var slotClipConfig = SlotClipConfigs[i];
        var done = input.IsDone() || input.GetTime() >= slotClipConfig.FadeOutEnd;
        if (done) {
          Mixer.DisconnectInput(i);
          Mixer.GetGraph().DestroySubgraph(input);
        }
      }
    }
  }
}