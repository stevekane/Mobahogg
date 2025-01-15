using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class RootMotionAnnihilatorTest : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] AnimationClip RootMotionAnimationClip;
  [SerializeField] bool KillRootMotion;
  [SerializeField] Transform Root;

  AnimationScriptPlayable KillRootMotionPlayable;
  PlayableGraph Graph;
  TransformStreamHandle RootHandle;

  KillRootMotionJob CurrentJob => new KillRootMotionJob {
    RootHandle = RootHandle,
    IgnoreRootMotion = KillRootMotion
  };

  void Start() {
    RootHandle = Animator.BindStreamTransform(Root);
    Graph = PlayableGraph.Create("Root Motion Annihilator");
    var animationClip = AnimationClipPlayable.Create(Graph, RootMotionAnimationClip);
    KillRootMotionPlayable = AnimationScriptPlayable.Create(Graph, CurrentJob, 1);
    KillRootMotionPlayable.ConnectInput(0, animationClip, 0, 1);
    var animationOutput = AnimationPlayableOutput.Create(Graph, "Animation Output", Animator);
    animationOutput.SetSourcePlayable(KillRootMotionPlayable);
    Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    Graph.Play();
  }

  void OnDestroy() {
    if (Graph.IsValid())
      Graph.Destroy();
  }

  void FixedUpdate() {
    KillRootMotionPlayable.SetJobData(CurrentJob);
    Graph.Evaluate(Time.fixedDeltaTime);
  }

  void OnGUI() {
    if (Selection.activeGameObject != gameObject)
      return;
    GUILayout.BeginVertical("box");
    GUILayout.Label($"Has Generic Root Transform: {RootMotionAnimationClip.hasGenericRootTransform}");
    GUILayout.Label($"Has Root Curves: {RootMotionAnimationClip.hasRootCurves}");
    GUILayout.Label($"Has Motion Curves: {RootMotionAnimationClip.hasMotionCurves}");
    GUILayout.Label($"Has Motion Float Curves: {RootMotionAnimationClip.hasMotionFloatCurves}");
    GUILayout.EndVertical();
  }
}

// Not really all that useful because Unity is fucking dogshit garbage and it does crazy things
// when the Root node for an animation is set to <None>.
// It seems to somehow all happen inside of Animator and thus whatever you do here is irrelevant
public struct KillRootMotionJob : IAnimationJob {
  public TransformStreamHandle RootHandle;
  public bool IgnoreRootMotion;
  public void ProcessRootMotion(AnimationStream stream) {
    var inputStream = stream.GetInputStream(0);
    if (IgnoreRootMotion || !inputStream.isValid) {
      stream.velocity = Vector3.zero;
    } else {
      stream.velocity = inputStream.velocity;
    }
  }

  // Pass through animation
  public void ProcessAnimation(AnimationStream stream) {
    stream.CopyAnimationStreamMotion(stream.GetInputStream(0));
    if (IgnoreRootMotion) {
      RootHandle.SetLocalTRS(stream, Vector3.zero, Quaternion.identity, Vector3.one, false);
    }
  }
}