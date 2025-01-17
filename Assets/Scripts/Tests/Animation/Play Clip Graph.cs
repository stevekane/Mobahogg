using System.Collections.Generic;
using Animation;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayClipGraph : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] List<AnimationClip> Clips;
  [SerializeField] int ActiveIndex;
  [SerializeField] float RootMotionScale = 1;

  PlayableGraph Graph;
  ScriptPlayable<SelectBehavior> SelectPlayable;
  AnimationScriptPlayable RootMotionScalePlayable;
  SelectBehavior SelectBehavior;
  AnimationPlayableOutput Output;

  void OnValidate() {
    ActiveIndex = Mathf.Clamp(ActiveIndex, 0, Clips.Count);
  }

  void Start() {
    var clipPlayable = AnimationClipPlayable.Create(Graph, Clips[0]);
    Graph = PlayableGraph.Create("Play Clip Graph");
    SelectPlayable = ScriptPlayable<SelectBehavior>.Create(Graph, Clips.Count);
    SelectBehavior = SelectPlayable.GetBehaviour();
    Clips.ForEach(c => SelectBehavior.Add(AnimationClipPlayable.Create(Graph, c)));
    RootMotionScalePlayable = AnimationScriptPlayable.Create(Graph, new RootMotionScaleJob(RootMotionScale), 0);
    RootMotionScalePlayable.AddInput(SelectPlayable, 0, 1);
    Output = AnimationPlayableOutput.Create(Graph, "Output", Animator);
    Output.SetSourcePlayable(Graph.GetRootPlayable(0));
    Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    Graph.Evaluate(0);
  }

  void FixedUpdate() {
    SelectBehavior.CrossFade(ActiveIndex, 100);
    RootMotionScalePlayable.SetJobData(new RootMotionScaleJob(RootMotionScale));
    Graph.Evaluate(Time.deltaTime);
  }
}