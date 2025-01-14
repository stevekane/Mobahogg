using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayClipGraph : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] AnimationClip Clip;

  PlayableGraph Graph;
  AnimationMixerPlayable Mixer;
  AnimationPlayableOutput Output;
  bool Active
    => !Mixer.GetInput(0).IsNull()
    && Mixer.GetInput(0).IsValid()
    && !Mixer.GetInput(0).IsDone()
    && Mixer.GetInput(0).GetTime() != 0;

  void Start() {
    Graph = PlayableGraph.Create("Play Clip Graph");
    Mixer = AnimationMixerPlayable.Create(Graph, 1);
    Output = AnimationPlayableOutput.Create(Graph, "Output", Animator);
    Output.SetSourcePlayable(Mixer);
    Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    Graph.Evaluate(0);
  }

  void FixedUpdate() {
    InputRouter.Instance.TryGetButtonState("Attack", 1, out var state);
    if (state == ButtonState.JustDown) {
      transform.position = Vector3.zero;
      var current = Mixer.GetInput(0);
      if (!current.IsNull()) {
        Graph.DestroySubgraph(current);
      }
      var clipPlayable = AnimationClipPlayable.Create(Graph, Clip);
      clipPlayable.SetTime(0);
      clipPlayable.SetDuration(Clip.length);
      Mixer.ConnectInput(0, clipPlayable, 0, 1);
      InputRouter.Instance.ConsumeButton("Attack", 1);
    }
    Graph.Evaluate(Time.deltaTime);
  }

  void OnAnimatorMove() {
    if (Active) {
      transform.position += Animator.deltaPosition;
      transform.rotation *= Animator.deltaRotation;
    }
  }
}