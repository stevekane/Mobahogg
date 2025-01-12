using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayClipGraph : MonoBehaviour {
  [SerializeField] AnimationClip Clip;
  [SerializeField] DirectorUpdateMode UpdateMode;
  [SerializeField] bool UseFootIk;

  PlayableGraph Graph;
  AnimationPlayableOutput Output;

  void Start() {
    Graph = PlayableGraph.Create("Play Clip Graph");
    Graph.SetTimeUpdateMode(UpdateMode);
    Output = AnimationPlayableOutput.Create(Graph, "Output", GetComponent<Animator>());
    Debug.Log($"Apparent Speed: {Clip.apparentSpeed:F5}");
    Debug.Log($"Average Speed: {Clip.averageSpeed:F5}");
    Debug.Log($"Average Duration: {Clip.averageDuration:F5}");
    Debug.Log($"Length: {Clip.length:F5}");
    Debug.Log($"Distance from ApparentSpeed: {Clip.apparentSpeed * Clip.averageDuration:F5}");
    Debug.Log($"Distance from AverageSpeed: {Clip.averageSpeed * Clip.averageDuration:F5}");
    Debug.Log($"Distance from apparent with humanScale: {Clip.apparentSpeed * Clip.averageDuration * GetComponent<Animator>().humanScale}");
    Debug.Log($"Distance from average with with humanScale: {Clip.averageSpeed * Clip.averageDuration * GetComponent<Animator>().humanScale}");
    Debug.Log($"{Clip.averageDuration} {Clip.length}");
    Debug.Log(GetComponent<Animator>().humanScale);
  }

  void FixedUpdate() {
    InputRouter.Instance.TryGetButtonState("Attack", 1, out var state);
    if (state == ButtonState.JustDown) {
      transform.position = Vector3.zero;
      var current = Output.GetSourcePlayable();
      if (!current.IsNull())
        current.Destroy();
      var clipPlayable = AnimationClipPlayable.Create(Graph, Clip);
      clipPlayable.SetApplyFootIK(UseFootIk);
      Output.SetSourcePlayable(clipPlayable);
      InputRouter.Instance.ConsumeButton("Attack", 1);
    }
    Graph.SetTimeUpdateMode(UpdateMode);
    if (Graph.GetTimeUpdateMode() == DirectorUpdateMode.Manual) {
      Graph.Evaluate(Time.deltaTime);
    }
  }
}
