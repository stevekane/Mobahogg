using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Unity.Collections;

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
    var attack = AnimationClipPlayable.Create(Graph, AttackingClip);
    attack.SetDuration(AttackingClip.length);
    var run = AnimationClipPlayable.Create(Graph, RunningClip);
    Slot.GetBehaviour().Add(attack);
    Slot.GetBehaviour().Add(run);
    Slot.GetBehaviour().ActiveIndex = SlotActiveIndex;
    var output = AnimationPlayableOutput.Create(Graph, "Animation Output", Animator);
    output.SetSourcePlayable(Slot);
    Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    Graph.Play();
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
    transform.position += Animator.deltaPosition;
    transform.rotation *= Animator.deltaRotation;
  }
}

class SimpleSlot : PlayableBehaviour {
  AnimationScriptPlayable AnimationScriptPlayable;
  NativeArray<TransformStreamHandle> Handles;

  public int ActiveIndex;

  TargetedMixerJob CurrentJob => new TargetedMixerJob {
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
    AnimationScriptPlayable.Destroy();
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

struct TargetedMixerJob : IAnimationJob {
  public NativeArray<TransformStreamHandle> Handles;
  public int ActiveIndex;

  public void ProcessRootMotion(AnimationStream stream) {
    stream.velocity = stream.GetInputStream(ActiveIndex).velocity;
    stream.angularVelocity = stream.GetInputStream(ActiveIndex).angularVelocity;
  }

  public void ProcessAnimation(AnimationStream stream) {
    var inputCount = stream.inputStreamCount;
    var numHandles = Handles.Length;
    var totalWeight = 0f;
    for (var i = 0; i < inputCount; i++) {
      totalWeight += stream.GetInputWeight(i);
    }

    if (Mathf.Approximately(totalWeight, 0))
      return;

    for (var i = 0; i < numHandles; i++) {
      var handle = Handles[i];
      var blendedPosition = Vector3.zero;
      var blendedRotation = new Quaternion(0,0,0,0);
      for (int j = 0; j < inputCount; j++) {
        var inputStream = stream.GetInputStream(j);
        var inputWeight = stream.GetInputWeight(j) / totalWeight;
        var localPosition = handle.GetLocalPosition(inputStream);
        var localRotation = handle.GetLocalRotation(inputStream);

        if (Quaternion.Dot(blendedRotation, localRotation) < 0f) {
          localRotation = new Quaternion(-localRotation.x, -localRotation.y, -localRotation.z, -localRotation.w);
        }

        blendedPosition += inputWeight * localPosition;
        blendedRotation.x += inputWeight * localRotation.x;
        blendedRotation.y += inputWeight * localRotation.y;
        blendedRotation.z += inputWeight * localRotation.z;
        blendedRotation.w += inputWeight * localRotation.w;
      }

      blendedRotation = blendedRotation.normalized;
      handle.SetLocalPosition(stream, blendedPosition);
      handle.SetLocalRotation(stream, blendedRotation);
    }

    bool outputHumanStream = stream.isHumanStream;
    bool allHumanInputStreams = true;
    for (var i = 0; i < inputCount; i++) {
      allHumanInputStreams = allHumanInputStreams && stream.GetInputStream(i).isHumanStream;
    }

    if (!outputHumanStream || !allHumanInputStreams)
      return;

    var blendedLeftFoot = Vector3.zero;
    var blendedLeftKnee = Vector3.zero;
    var blendedRightFoot = Vector3.zero;
    var blendedRightKnee = Vector3.zero;
    var blendedLeftFootRotation = new Quaternion(0, 0, 0, 0);
    var blendedRightFootRotation = new Quaternion(0, 0, 0, 0);
    var blendedLeftFootWeight = 0f;
    var blendedLeftKneeWeight = 0f;
    var blendedRightFootWeight = 0f;
    var blendedRightKneeWeight = 0f;
    var blendedLeftFootRotationWeight = 0f;
    var blendedRightFootRotationWeight = 0f;

    for (var i = 0; i < inputCount; i++) {
      var humanInputStream = stream.GetInputStream(i).AsHuman();
      var inputWeight = stream.GetInputWeight(i) / totalWeight;

      var leftFoot = humanInputStream.GetGoalLocalPosition(AvatarIKGoal.LeftFoot);
      var rightFoot = humanInputStream.GetGoalLocalPosition(AvatarIKGoal.RightFoot);
      var leftKnee = humanInputStream.GetHintPosition(AvatarIKHint.LeftKnee);
      var rightKnee = humanInputStream.GetHintPosition(AvatarIKHint.RightKnee);

      var leftFootRotation = humanInputStream.GetGoalLocalRotation(AvatarIKGoal.LeftFoot);
      var rightFootRotation = humanInputStream.GetGoalLocalRotation(AvatarIKGoal.RightFoot);

      var leftFootWeight = humanInputStream.GetGoalWeightPosition(AvatarIKGoal.LeftFoot);
      var rightFootWeight = humanInputStream.GetGoalWeightPosition(AvatarIKGoal.RightFoot);
      var leftFootRotationWeight = humanInputStream.GetGoalWeightRotation(AvatarIKGoal.LeftFoot);
      var rightFootRotationWeight = humanInputStream.GetGoalWeightRotation(AvatarIKGoal.RightFoot);
      var leftKneeWeight = humanInputStream.GetHintWeightPosition(AvatarIKHint.LeftKnee);
      var rightKneeWeight = humanInputStream.GetHintWeightPosition(AvatarIKHint.RightKnee);

      blendedLeftFoot += inputWeight * leftFoot;
      blendedRightFoot += inputWeight * rightFoot;
      blendedLeftKnee += inputWeight * leftKnee;
      blendedRightKnee += inputWeight * rightKnee;

      blendedLeftFootWeight += inputWeight * leftFootWeight;
      blendedRightFootWeight += inputWeight * rightFootWeight;
      blendedLeftKneeWeight += inputWeight * leftKneeWeight;
      blendedRightKneeWeight += inputWeight * rightKneeWeight;

      blendedLeftFootRotationWeight += inputWeight * leftFootRotationWeight;
      blendedRightFootRotationWeight += inputWeight * rightFootRotationWeight;

      if (Quaternion.Dot(blendedLeftFootRotation, leftFootRotation) < 0f) {
        leftFootRotation = new Quaternion(-leftFootRotation.x, -leftFootRotation.y, -leftFootRotation.z, -leftFootRotation.w);
      }
      blendedLeftFootRotation.x += inputWeight * leftFootRotation.x;
      blendedLeftFootRotation.y += inputWeight * leftFootRotation.y;
      blendedLeftFootRotation.z += inputWeight * leftFootRotation.z;
      blendedLeftFootRotation.w += inputWeight * leftFootRotation.w;

      if (Quaternion.Dot(blendedRightFootRotation, rightFootRotation) < 0f) {
        rightFootRotation = new Quaternion(-rightFootRotation.x, -rightFootRotation.y, -rightFootRotation.z, -rightFootRotation.w);
      }
      blendedRightFootRotation.x += inputWeight * rightFootRotation.x;
      blendedRightFootRotation.y += inputWeight * rightFootRotation.y;
      blendedRightFootRotation.z += inputWeight * rightFootRotation.z;
      blendedRightFootRotation.w += inputWeight * rightFootRotation.w;
    }

    blendedLeftFootRotation = blendedLeftFootRotation.normalized;
    blendedRightFootRotation = blendedRightFootRotation.normalized;

    var humanOutputStream = stream.AsHuman();
    humanOutputStream.SetGoalLocalPosition(AvatarIKGoal.LeftFoot, blendedLeftFoot);
    humanOutputStream.SetGoalLocalPosition(AvatarIKGoal.RightFoot, blendedRightFoot);
    humanOutputStream.SetGoalLocalRotation(AvatarIKGoal.LeftFoot, blendedLeftFootRotation);
    humanOutputStream.SetGoalLocalRotation(AvatarIKGoal.RightFoot, blendedRightFootRotation);
    humanOutputStream.SetHintPosition(AvatarIKHint.LeftKnee, blendedLeftKnee);
    humanOutputStream.SetHintPosition(AvatarIKHint.RightKnee, blendedRightKnee);

    humanOutputStream.SetGoalWeightPosition(AvatarIKGoal.LeftFoot, blendedLeftFootWeight);
    humanOutputStream.SetGoalWeightPosition(AvatarIKGoal.RightFoot, blendedRightFootWeight);
    humanOutputStream.SetGoalWeightRotation(AvatarIKGoal.LeftFoot, blendedLeftFootRotationWeight);
    humanOutputStream.SetGoalWeightRotation(AvatarIKGoal.RightFoot, blendedRightFootRotationWeight);
    humanOutputStream.SetHintWeightPosition(AvatarIKHint.LeftKnee, blendedLeftKneeWeight);
    humanOutputStream.SetHintWeightPosition(AvatarIKHint.RightKnee, blendedRightKneeWeight);
  }
}