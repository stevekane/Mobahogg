using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Animation {
  [CreateAssetMenu(menuName = "AnimationGraph/Slot")]
  public class SlotAsset : PlayableAsset {
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
      return ScriptPlayable<SlotBehavior>.Create(graph, 1);
    }
  }

  public class SlotBehavior : PlayableBehaviour {
    public bool IsRunning { get; private set; }
    public AnimationClipPlayable? ActivePlayable { get; private set; }

    Playable Playable;
    AnimationLayerMixerPlayable LayerMixer;
    AnimationMixerPlayable Mixer;

    // Called to establish the AnimationStream that comes into this slot
    public void Connect(Playable playable) {
      LayerMixer.ConnectInput(0, playable, 0, 1);
    }

    // Called to play an animation on this slot overriding whatever is Connected
    public void Play(AnimationClipPlayable playable) {
      Stop();
      ActivePlayable = playable;
      Mixer.AddInput(playable, 0, 1);
    }

    public void Stop() {
      if (Mixer.GetInputCount() > 0) {
        var existing = Mixer.GetInput(0);
        Mixer.DisconnectInput(0);
        Mixer.GetGraph().DestroySubgraph(existing);
        Mixer.SetInputCount(0);
        ActivePlayable = null;
      }
    }

    public override void OnPlayableCreate(Playable playable) {
      LayerMixer = AnimationLayerMixerPlayable.Create(playable.GetGraph(), 2);
      Mixer = AnimationMixerPlayable.Create(playable.GetGraph(), 0);
      Playable = playable;
      LayerMixer.ConnectInput(1, Mixer, 0, 1);
      Playable.ConnectInput(0, LayerMixer, 0, 1);
    }

    public override void OnPlayableDestroy(Playable playable) {
      playable.GetGraph().DestroySubgraph(Mixer);
    }

    public override void PrepareFrame(Playable playable, FrameData info) {
      IsRunning = false;
      var count = Mixer.GetInputCount();
      for (var i = 0; i < count; i++) {
        var input = Mixer.GetInput(i);
        var done = input.IsDone();
        IsRunning = IsRunning || !done;
        if (done) {
          Mixer.DisconnectInput(i);
          Mixer.SetInputCount(0);
          Mixer.GetGraph().DestroySubgraph(input);
        }
      }
      LayerMixer.SetInputWeight(1, IsRunning ? 1 : 0);
    }
  }
}