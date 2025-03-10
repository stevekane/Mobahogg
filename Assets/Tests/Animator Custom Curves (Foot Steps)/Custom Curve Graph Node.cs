using Animation;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class CustomCurveGraphNode : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] AnimationClip Idle;
  [SerializeField] AnimationClip Jog;
  [SerializeField] AnimationClip Run;
  [SerializeField] AudioSource AudioSource;
  [SerializeField, Range(0,2)] int Index;
  [SerializeField, Range(0,1)] float Speed = 1;
  [SerializeField] GameObject FootStepVFX;

  PlayableGraph Graph;
  ScriptPlayable<SelectBehavior> Selector;
  AnimationScriptPlayable FootSteps;

  void Awake() {
    Graph = PlayableGraph.Create("Custom Curve");
    var output = AnimationPlayableOutput.Create(Graph, "Animator", Animator);
    Selector = ScriptPlayable<SelectBehavior>.Create(Graph, 1);
    Selector.GetBehaviour().Add(AnimationClipPlayable.Create(Graph, Idle));
    Selector.GetBehaviour().Add(AnimationClipPlayable.Create(Graph, Jog));
    Selector.GetBehaviour().Add(AnimationClipPlayable.Create(Graph, Run));
    var hasFootStepHandle = Animator.BindCustomStreamProperty("HasFootStep", CustomStreamPropertyType.Bool);
    var footStepHandle = Animator.BindCustomStreamProperty("FootStep", CustomStreamPropertyType.Float);
    FootSteps = AnimationScriptPlayable.Create(Graph, new ExtractFootsteps(hasFootStepHandle, footStepHandle), 1);
    FootSteps.ConnectInput(0, Selector, 0);
    output.SetSourcePlayable(FootSteps, 0);
    Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
  }

  void OnDestroy() {
    if (Graph.IsValid())
      Graph.Destroy();
  }

  void FixedUpdate() {
    Selector.GetBehaviour().CrossFade(Index);
    Selector.SetSpeed(Speed);
    Graph.Evaluate(Time.fixedDeltaTime);
    if (FootSteps.GetJobData<ExtractFootsteps>().FootStepOccured) {
      var leftFoot = Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
      var rightFoot = Animator.GetBoneTransform(HumanBodyBones.RightFoot);
      var location = leftFoot.transform.position.y < rightFoot.transform.position.y
        ? leftFoot
        : rightFoot;
      var vfx = Instantiate(FootStepVFX, location.position, transform.rotation);
      Destroy(vfx, 3);
      AudioSource.PlayOneShot(AudioSource.clip);
    }
  }
}

public struct ExtractFootsteps : IAnimationJob {
  static bool CrossedZero(float prev, float cur) {
    if (prev < 0 && cur >= 0) return true;
    if (prev > 0 && cur <= 0) return true;
    if (prev <= 0 && cur > 0) return true;
    if (prev >= 0 && cur < 0) return true;
    return false;
  }

  public PropertyStreamHandle HasFootStepHandle;
  public PropertyStreamHandle FootStepHandle;
  public bool FootStepOccured;
  public bool WasTracking;

  float LastFootStepValue;

  public ExtractFootsteps(PropertyStreamHandle hasFootStepHandle, PropertyStreamHandle footStepHandle) {
    HasFootStepHandle = hasFootStepHandle;
    FootStepHandle = footStepHandle;
    WasTracking = false;
    FootStepOccured = false;
    LastFootStepValue = 0;
  }

  public void ProcessAnimation(AnimationStream stream) {
    if (HasFootStepHandle.GetBool(stream)) {
      var footStepValue = FootStepHandle.GetFloat(stream);
      FootStepOccured = WasTracking && CrossedZero(LastFootStepValue, footStepValue);
      LastFootStepValue = footStepValue;
      WasTracking = true;
    } else {
      LastFootStepValue = 0;
      FootStepOccured = false;
      WasTracking = false;
    }
  }

  public void ProcessRootMotion(AnimationStream stream) {}
}