using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class RootMotionAnnihilatorTest : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] AnimationClip RootMotionAnimationClip;
  [SerializeField] bool KillRootMotion;

  AnimationScriptPlayable KillRootMotionPlayable;
  PlayableGraph Graph;

  void Start() {
    Graph = PlayableGraph.Create("Root Motion Annihilator");
    var animationClip = AnimationClipPlayable.Create(Graph, RootMotionAnimationClip);
    KillRootMotionPlayable = AnimationScriptPlayable.Create(Graph, new KillRootMotionJob(), 1);
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
    KillRootMotionPlayable.SetJobData(new KillRootMotionJob { IgnoreRootMotion = KillRootMotion });
    Graph.Evaluate(Time.fixedDeltaTime);
  }
}

public struct KillRootMotionJob : IAnimationJob {
  public bool IgnoreRootMotion;
  public void ProcessRootMotion(AnimationStream stream) {
    var inputStream = stream.GetInputStream(0);
    if (IgnoreRootMotion || !inputStream.isValid) {
      stream.velocity = Vector3.zero;
      stream.angularVelocity = Vector3.zero;
    } else {
      stream.velocity = inputStream.velocity;
      stream.angularVelocity = inputStream.angularVelocity;
    }
  }

  // Pass through animation
  public void ProcessAnimation(AnimationStream stream) {
    stream.CopyAnimationStreamMotion(stream.GetInputStream(0));
  }
}