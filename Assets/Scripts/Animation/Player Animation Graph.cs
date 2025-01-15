using System.Linq;
using System;
using Animation;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class PlayerAnimationGraph : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Animator Animator;
  [Header("Grounded Locomotion")]
  [SerializeField] AnimationClip GroundedIdleClip;
  [SerializeField] AnimationClip[] GroundMovingClips;
  [SerializeField] float[] GroundMovingClipVelocities;
  [Header("Airborne Locomotion")]
  [SerializeField] AnimationClip AirborneHoverAnimationClip;
  [Header("Slot Animations")]
  [SerializeField] AnimationMontage AttackMontage;
  [SerializeField] AnimationMontage DashMontage;
  [SerializeField] AnimationClip AttackAnimationClip;
  [SerializeField] AnimationClip DiveRollAnimationClip;
  [SerializeField] float AttackRootMotionScalar = 3;
  [SerializeField] float DiveRollRootMotionScalar = 3;
  [Header("Transitions")]
  [SerializeField] float TransitionSpeed = 4;
  [SerializeField] float SlotCrossFadeDuration = 0.15f;

  public bool Grounded;
  [Range(0, 10)]
  public float LocomotionSpeed;

  AnimationClipPlayable[] GroundMovingClipPlayables;
  ScriptPlayable<SelectBehavior> LocomotionSelect;
  ScriptPlayable<SelectBehavior> GroundedSelect;
  ScriptPlayable<SelectBehavior> GroundedIdleSelect;
  ScriptPlayable<SelectBehavior> GroundedMovingSelect;
  ScriptPlayable<SelectBehavior> AirborneSelect;
  ScriptPlayable<SlotBehavior> Slot;
  Playable CurrentAttackMontagePlayable;
  Playable CurrentDashMontagePlayable;

  PlayableGraph Graph;

  #if UNITY_EDITOR
  [ContextMenu("Calculate Clip Velocities")]
  void CalculateClipVelocities() {
    GroundMovingClipVelocities = new float[GroundMovingClips.Length];
    for (var i = 0; i < GroundMovingClips.Length; i++)  {
      GroundMovingClipVelocities[i] = GroundMovingClips[i].apparentSpeed;
    }
  }

  void OnValidate() {
    CalculateClipVelocities();
  }
  #endif

  void Start() {
    Graph = PlayableGraph.Create($"{name}.PlayerAnimationGraph");
    GroundMovingClipPlayables = GroundMovingClips.Select(clip => AnimationClipPlayable.Create(Graph, clip)).ToArray();
    GroundedMovingSelect = ScriptPlayable<SelectBehavior>.Create(Graph, 1);
    GroundMovingClipPlayables.ForEach(clipPlayable => GroundedMovingSelect.GetBehaviour().Add(clipPlayable));
    GroundedIdleSelect = ScriptPlayable<SelectBehavior>.Create(Graph, 1);
    GroundedIdleSelect.GetBehaviour().Add(AnimationClipPlayable.Create(Graph, GroundedIdleClip));
    GroundedSelect = ScriptPlayable<SelectBehavior>.Create(Graph, 1);
    GroundedSelect.GetBehaviour().Add(GroundedIdleSelect);
    GroundedSelect.GetBehaviour().Add(GroundedMovingSelect);

    var airborneHover = AnimationClipPlayable.Create(Graph, AirborneHoverAnimationClip);
    AirborneSelect = ScriptPlayable<SelectBehavior>.Create(Graph, 1);
    AirborneSelect.GetBehaviour().Add(airborneHover);

    LocomotionSelect = ScriptPlayable<SelectBehavior>.Create(Graph, 1);
    LocomotionSelect.GetBehaviour().Add(GroundedSelect);
    LocomotionSelect.GetBehaviour().Add(AirborneSelect);

    Slot = ScriptPlayable<SlotBehavior>.Create(Graph, 1);
    Slot.SetOutputCount(2);
    Slot.GetBehaviour().Connect(LocomotionSelect);
    var animationOutput = AnimationPlayableOutput.Create(Graph, $"AnimationOutput({Animator.name})", Animator);
    animationOutput.SetSourcePlayable(Slot, 0);
    var scriptOutput = ScriptPlayableOutput.Create(Graph, $"ScriptOutput({name})");
    scriptOutput.SetUserData(this);
    scriptOutput.SetSourcePlayable(Slot, 1);
    Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    Graph.Evaluate(0);
  }

  void OnDestroy() {
    if (Graph.IsValid())
      Graph.Destroy();
  }

  void FixedUpdate() {
    InputRouter.Instance.TryGetValue("Move", 0, out var move);
    InputRouter.Instance.TryGetButtonState("Attack", 0, out var attackState);
    InputRouter.Instance.TryGetButtonState("Dash", 0, out var dashState);
    InputRouter.Instance.TryGetButtonState("Cast Spell", 0, out var castSpellState);

    if (attackState == ButtonState.JustDown) {
      if (move.sqrMagnitude > 0)
        transform.rotation = Quaternion.LookRotation(move.XZ().normalized);
      CurrentAttackMontagePlayable = AttackMontage.CreateScriptPlayable(Graph);
      Slot.GetBehaviour().Play(CurrentAttackMontagePlayable);
      InputRouter.Instance.ConsumeButton("Attack", 0);
    }

    if (dashState == ButtonState.JustDown) {
      if (move.sqrMagnitude > 0)
        transform.rotation = Quaternion.LookRotation(move.XZ().normalized);
      CurrentDashMontagePlayable = DashMontage.CreateScriptPlayable(Graph);
      Slot.GetBehaviour().Play(CurrentDashMontagePlayable);
      InputRouter.Instance.ConsumeButton("Dash", 0);
    }

    if (castSpellState == ButtonState.JustDown) {
      transform.position = Vector3.zero;
      InputRouter.Instance.ConsumeButton("Cast Spell", 0);
    }

    if (move.sqrMagnitude > 0 && !Slot.GetBehaviour().IsRunning) {
      transform.rotation = Quaternion.LookRotation(move.XZ().normalized);
      transform.position = transform.position + LocalClock.DeltaTime() * LocomotionSpeed * transform.forward;
    }

    LocomotionSpeed = 10 * move.magnitude;

    // We can read inputs here to simulate attacking
    LocomotionSelect.GetBehaviour().CrossFade(Grounded ? 0 : 1, TransitionSpeed);
    GroundedSelect.GetBehaviour().CrossFade(LocomotionSpeed <= 0 ? 0 : 1, TransitionSpeed);
    if (LocomotionSpeed > 0) {
      var bestFittingGroundClipIndex = IndexWithBestSpeedMatch(GroundMovingClipVelocities, LocomotionSpeed);
      var clipPlayable = GroundMovingClipPlayables[bestFittingGroundClipIndex];
      var clipRootSpeed = GroundMovingClipVelocities[bestFittingGroundClipIndex];
      clipPlayable.SetSpeed(LocomotionSpeed / clipRootSpeed);
      GroundedMovingSelect.GetBehaviour().CrossFade(bestFittingGroundClipIndex, true, TransitionSpeed);
    }
    Slot.GetBehaviour().FadeDuration = SlotCrossFadeDuration;
    AirborneSelect.GetBehaviour().CrossFade(0, 1);
    Graph.Evaluate(LocalClock.DeltaTime());
  }

  void OnNotifyStart(AnimationNotify notify) {
    Debug.Log($"{notify.Name}.Start {TimeManager.Instance.FixedFrame()}");
  }

  void OnNotifyEnd(AnimationNotify notify) {
    Debug.Log($"{notify.Name}.End {TimeManager.Instance.FixedFrame()}");
  }

  void OnAnimatorMove() {
    var slot = Slot.GetBehaviour();
    if (slot.ApplyRootMotion) {
      var speed =
        slot.ActivePlayable.Equals(CurrentAttackMontagePlayable)
          ? AttackRootMotionScalar
          : slot.ActivePlayable.Equals(CurrentDashMontagePlayable)
            ? DiveRollRootMotionScalar
            : 1;
      transform.position += speed * Animator.deltaPosition;
    }
  }

  // Do not pass empty array unless you want to cry or check for -1 at call-site
  int IndexWithBestSpeedMatch(float[] clipSpeeds, float speed) {
    int bestIndex = -1;
    float lowestDistance = float.MaxValue;
    for (var i = 0; i < clipSpeeds.Length; i++) {
      var clipSpeed = clipSpeeds[i];
      var speedDistance = Mathf.Abs(clipSpeed-speed);
      if (speedDistance < lowestDistance) {
        bestIndex = i;
        lowestDistance = speedDistance;
      }
    }
    return bestIndex;
  }
}