using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayClipGraph : MonoBehaviour {
  [SerializeField] AnimationClip Clip;
  [SerializeField] DirectorUpdateMode UpdateMode;

  PlayableGraph Graph;
  AnimationPlayableOutput Output;

  void Start() {
    Graph = PlayableGraph.Create("Play Clip Graph");
    Graph.SetTimeUpdateMode(UpdateMode);
    Output = AnimationPlayableOutput.Create(Graph, "Output", GetComponent<Animator>());
  }

  void FixedUpdate() {
    InputRouter.Instance.TryGetButtonState("Attack", 1, out var state);
    if (state == ButtonState.JustDown) {
      transform.position = Vector3.zero;
      var current = Output.GetSourcePlayable();
      if (!current.IsNull())
        current.Destroy();
      Output.SetSourcePlayable(AnimationClipPlayable.Create(Graph, Clip));
      InputRouter.Instance.ConsumeButton("Attack", 1);
    }
    Graph.SetTimeUpdateMode(UpdateMode);
    if (Graph.GetTimeUpdateMode() == DirectorUpdateMode.Manual) {
      Graph.Evaluate(Time.deltaTime);
    }
  }
}
