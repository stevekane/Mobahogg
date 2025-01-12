using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Unity.Collections;
using Animation;

public class AnimationBlendingTest : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] AnimationClip RunningClip;
  [SerializeField] AnimationClip AttackingClip;
  [SerializeField] int SlotActiveIndex = 0;

  PlayableGraph Graph;
  ScriptPlayable<SimpleSlot> Slot;

  void Start() {
    Graph = PlayableGraph.Create("Animation Blending");
    Slot = ScriptPlayable<SimpleSlot>.Create(Graph, 0);
    var run = AnimationClipPlayable.Create(Graph, RunningClip);
    Slot.GetBehaviour().Add(run);
    Slot.GetBehaviour().ActiveIndex = SlotActiveIndex;
    Attack();
    var output = AnimationPlayableOutput.Create(Graph, "Animation Output", Animator);
    output.SetSourcePlayable(Slot);
    Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    Graph.Play();
  }

  void Attack() {
    var attack = AnimationClipPlayable.Create(Graph, AttackingClip);
    attack.SetTime(0);
    attack.SetDuration(AttackingClip.length);
    Slot.GetBehaviour().Add(attack);
    Graph.Evaluate(0);
  }

  void OnDestroy() {
    if (Graph.IsValid())
      Graph.Destroy();
  }

  void FixedUpdate() {
    Slot.GetBehaviour().ActiveIndex = SlotActiveIndex;
    Graph.Evaluate(Time.deltaTime);
  }

  void OnAnimatorMove() {
    if (Animator.deltaPosition.sqrMagnitude > 0) {
      Debug.Log($"{Animator.deltaPosition.z:F5}");
    }
    transform.position += Animator.deltaPosition;
    transform.rotation *= Animator.deltaRotation;
  }
}

class SimpleSlot : PlayableBehaviour {
  AnimationScriptPlayable AnimationScriptPlayable;
  NativeArray<TransformStreamHandle> Handles;

  public int ActiveIndex;

  SlotMixerJob CurrentJob => new SlotMixerJob {
    Handles = Handles,
    ActiveIndex = ActiveIndex
  };

  public void Add(Playable playable) {
    AnimationScriptPlayable.AddInput(playable, 0, 1);
  }

  public override void OnPlayableCreate(Playable playable) {
    AnimationScriptPlayable = AnimationScriptPlayable.Create(playable.GetGraph(), CurrentJob, 0);
    playable.AddInput(AnimationScriptPlayable, 0, 1);
  }

  public override void OnPlayableDestroy(Playable playable) {
    Handles.Dispose();
    playable.GetGraph().DestroySubgraph(AnimationScriptPlayable);
  }

  public override void OnGraphStart(Playable playable) {
    var animationOutput = (AnimationPlayableOutput)playable.GetGraph().GetOutputByType<AnimationPlayableOutput>(0);
    if (!animationOutput.IsOutputNull() && animationOutput.IsOutputValid() && animationOutput.GetTarget() is var animator && animator) {
      var transforms = animator.GetComponentsInChildren<Transform>();
      var numTransforms = transforms.Length-1;
      Handles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.ClearMemory);
      for (var i = 0; i < numTransforms; i++) {
        Handles[i] = animator.BindStreamTransform(transforms[i+1]);
      }
    }
  }

  public override void PrepareFrame(Playable playable, FrameData info) {
    AnimationScriptPlayable.SetJobData(CurrentJob);
  }
}
