using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Animation {
  [CreateAssetMenu(menuName = "AnimationGraph/Select")]
  public class SelectAsset : PlayableAsset {
    [SerializeField] PlayableAsset[] Cases;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
      var playable = ScriptPlayable<SelectBehavior>.Create(graph, 1);
      var select = playable.GetBehaviour();
      foreach (var c in Cases) {
        select.Add(c.CreatePlayable(graph, owner));
      }
      return playable;
    }
  }

  public class SelectBehavior : PlayableBehaviour {
    const float DEFAULT_SPEED = 4;

    Playable Playable;
    AnimationMixerPlayable Mixer;
    float Speed = DEFAULT_SPEED;
    int Index;

    public void CrossFade(int index, float speed = DEFAULT_SPEED) => CrossFade(index, false, speed);

    public void CrossFade(int index, bool MatchNormalizedTime, float speed = DEFAULT_SPEED) {
      if (Index != index) {
        var next = Mixer.GetInput(index);
        if (MatchNormalizedTime) {
          var current = Mixer.GetInput(Index);
          var currentTime = current.GetTime();
          var currentDuration = current.GetDuration();
          var nextDuration = next.GetDuration();
          next.SetTime(currentTime / currentDuration * nextDuration);
        } else {
          next.SetTime(0);
        }
        next.Play();
      }
      Index = index;
      Speed = speed;
    }

    public void Add(Playable playable) {
      Mixer.AddInput(playable, sourceOutputIndex: 0, weight: Mixer.GetInputCount() == 0 ? 1 : 0);
    }

    public override void OnPlayableCreate(Playable playable) {
      Mixer = AnimationMixerPlayable.Create(playable.GetGraph(), 0);
      Playable = playable;
      Playable.ConnectInput(0, Mixer, 0, 1);
    }

    public override void OnPlayableDestroy(Playable playable) {
      playable.GetGraph().DestroySubgraph(Mixer);
    }

    public override void PrepareFrame(Playable playable, FrameData info) {
      var count = Mixer.GetInputCount();
      for (var i = 0; i < count; i++) {
        var target = i == Index ? 1 : 0;
        var weight = Mixer.GetInputWeight(i);
        var input = Mixer.GetInput(i);
        weight = Mathf.MoveTowards(weight, target, info.deltaTime * Speed);
        Mixer.SetInputWeight(i, weight);
        if (weight == 0 && input.GetPlayState().Equals(PlayState.Playing)) {
          input.Pause();
        } else if (weight != 0 && input.GetPlayState().Equals(PlayState.Paused)) {
          input.Play();
        }
      }
    }
  }
}