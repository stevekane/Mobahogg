using Unity.Collections;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine;

namespace Animation {
  struct SlotClipConfig {
    public double FadeOutStart;
    public double FadeOutEnd;
  }

  struct SlotMixerJob : IAnimationJob {
    public NativeArray<TransformStreamHandle> Handles;
    public int ActiveIndex;

    // Custom Root Motion blending. Takes Root Motion only from Active Stream by Index
    public void ProcessRootMotion(AnimationStream stream) {
      var inputStream = stream.GetInputStream(ActiveIndex);
      if (inputStream.isValid) {
        stream.velocity = inputStream.velocity;
        stream.angularVelocity = inputStream.angularVelocity;
      }
    }

    // Bog standard animation and IK mixing... could be abstracted / extracted?
    public void ProcessAnimation(AnimationStream stream) {
      var inputCount = stream.inputStreamCount;
      var numHandles = Handles.Length;
      var totalWeight = 0f;
      for (var i = 0; i < inputCount; i++) {
        var inputStream = stream.GetInputStream(i);
        if (inputStream.isValid) {
          totalWeight += stream.GetInputWeight(i);
        }
      }

      if (Mathf.Approximately(totalWeight, 0))
        return;

      for (var i = 0; i < numHandles; i++) {
        var handle = Handles[i];
        var blendedPosition = Vector3.zero;
        var blendedRotation = new Quaternion(0,0,0,0);
        for (int j = 0; j < inputCount; j++) {
          var inputStream = stream.GetInputStream(j);
          if (inputStream.isValid) {
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
        }

        blendedRotation = blendedRotation.normalized;
        handle.SetLocalPosition(stream, blendedPosition);
        handle.SetLocalRotation(stream, blendedRotation);
      }

      bool outputHumanStream = stream.isHumanStream;
      bool allHumanInputStreams = true;
      for (var i = 0; i < inputCount; i++) {
        var inputStream = stream.GetInputStream(i);
        if (inputStream.isValid) {
          allHumanInputStreams = allHumanInputStreams && inputStream.isHumanStream;
        }
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
        var inputStream = stream.GetInputStream(i);
        if (inputStream.isValid) {
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
  }

  public class SlotBehavior : PlayableBehaviour {
    public bool IsRunning => ActiveIndex > 0;
    public bool ApplyRootMotion => IsRunning && ActivePlayable.GetTime() != 0;
    public Playable ActivePlayable => Mixer.GetInput(ActiveIndex);
    public double FadeDuration = 0.25f;

    AnimationScriptPlayable Mixer;
    NativeArray<TransformStreamHandle> Handles;
    SlotClipConfig[] SlotClipConfigs = new SlotClipConfig[32];
    int FadeInIndex;
    int ActiveIndex;

    SlotMixerJob CurrentJob => new SlotMixerJob {
      Handles = Handles,
      ActiveIndex = ActiveIndex,
    };

    int? OpenPort {
      get {
        var portCount = Mixer.GetInputCount();
        for (var i = 1; i < portCount; i++) {
          var input = Mixer.GetInput(i);
          if (input.IsNull() || !input.IsValid()) {
            return i;
          }
        }
        return null;
      }
    }

    public void Connect(Playable playable) {
      Mixer.ConnectInput(0, playable, 0, 1);
    }

    public void Play(Playable playable) {
      playable.SetTime(0);
      if (IsRunning) {
        var active = Mixer.GetInput(ActiveIndex);
        SlotClipConfigs[ActiveIndex].FadeOutStart = active.GetTime();
        SlotClipConfigs[ActiveIndex].FadeOutEnd = active.GetTime() + FadeDuration;
      }
      var openPort = OpenPort;
      if (openPort.HasValue) {
        ActiveIndex = openPort.Value;
        Mixer.ConnectInput(openPort.Value, playable, 0, 0);
      } else {
        ActiveIndex = Mixer.GetInputCount();
        Mixer.AddInput(playable, 0, 0);
      }
      FadeInIndex = ActiveIndex;
      SlotClipConfigs[ActiveIndex].FadeOutStart = playable.GetDuration()-FadeDuration;
      SlotClipConfigs[ActiveIndex].FadeOutEnd = playable.GetDuration();
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

    // TODO: Add Stop which is sort of like playing nothing new
    public override void OnPlayableCreate(Playable playable) {
      Mixer = AnimationScriptPlayable.Create(playable.GetGraph(), CurrentJob, 1);
      playable.ConnectInput(0, Mixer, 0, 1);
    }

    public override void OnPlayableDestroy(Playable playable) {
      Handles.Dispose();
      playable.GetGraph().DestroySubgraph(Mixer);
    }

    public override void PrepareFrame(Playable playable, FrameData info) {
      var count = Mixer.GetInputCount();

      // Check if we have reached fade start for the current active playable
      if (IsRunning) {
        var activePlayable = Mixer.GetInput(ActiveIndex);
        var activeSlotClipConfig = SlotClipConfigs[ActiveIndex];
        if (!activePlayable.IsNull() && activePlayable.GetTime() >= activeSlotClipConfig.FadeOutStart) {
          FadeInIndex = 0;
        }
      }

      // Update fade values
      for (var i = 0; i < count; i++) {
        var currentWeight = Mixer.GetInputWeight(i);
        var targetWeight = i == FadeInIndex ? 1 : 0;
        var nextWeight = Mathf.MoveTowards(currentWeight, targetWeight, info.deltaTime / (float)FadeDuration);
        Mixer.SetInputWeight(i, nextWeight);
      }

      // Remove any completed clips
      for (var i = count-1; i > 0; i--) {
        var input = Mixer.GetInput(i);
        if (input.IsNull() || !input.IsValid())
          continue;
        var slotClipConfig = SlotClipConfigs[i];
        var done = input.IsDone() || input.GetTime() >= slotClipConfig.FadeOutEnd;
        if (done) {
          // This is the active clip completing so set ActiveIndex to 0
          if (i == ActiveIndex) {
            ActiveIndex = 0;
          }
          Mixer.DisconnectInput(i);
          Mixer.GetGraph().DestroySubgraph(input);
        }
      }
      Mixer.SetJobData(CurrentJob);
    }
  }
}