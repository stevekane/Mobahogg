using System.Linq;
using System;
using Animation;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.InputSystem.Interactions;

[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class PlayerAnimationGraph : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Animator Animator;
  [Header("Grounded Locomotion")]
  [SerializeField] AnimationClip GroundedIdleClip;
  [SerializeField] AnimationClip[] GroundMovingClips;
  [Header("Airborne Locomotion")]
  [SerializeField] AnimationClip AirborneHoverAnimationClip;
  [Header("Slot Animations")]
  [SerializeField] AnimationClip AttackAnimationClip;
  [SerializeField] AnimationClip DiveRollAnimationClip;
  [SerializeField] float AttackSpeed = 1.5f;
  [SerializeField] float AttackRootMotionScalar = 3;
  [SerializeField] float DiveRollRootMotionScalar = 3;
  [SerializeField] float DiveRollSpeed = 2;
  [Header("Transitions")]
  [SerializeField] float TransitionSpeed = 4;
  [SerializeField] float SpeedChangeSpeed = 0.25f;

  public bool Grounded;
  [Range(0, 10)]
  public float LocomotionSpeed;
  public float SmoothLocomotionSpeed;

  AnimationClipPlayable[] GroundMovingClipPlayables;
  ScriptPlayable<SelectBehavior> LocomotionSelect;
  ScriptPlayable<SelectBehavior> GroundedSelect;
  ScriptPlayable<SelectBehavior> GroundedIdleSelect;
  ScriptPlayable<SelectBehavior> GroundedMovingSelect;
  ScriptPlayable<SelectBehavior> AirborneSelect;
  ScriptPlayable<SlotBehavior> Slot;

  PlayableGraph Graph;

  void Awake() {
    Graph = PlayableGraph.Create($"{name}.PlayerAnimationGraph");
    // TODO: Probably want to configure these a bit more to activate IK and stuff
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
    Slot.GetBehaviour().Connect(LocomotionSelect);
    var animationOutput = AnimationPlayableOutput.Create(Graph, $"{Animator.name}.Animator", Animator);
    animationOutput.SetSourcePlayable(Slot);
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
      var clipPlayable = AnimationClipPlayable.Create(Graph, AttackAnimationClip);
      clipPlayable.SetApplyFootIK(true);
      clipPlayable.SetApplyPlayableIK(true);
      clipPlayable.SetDuration(AttackAnimationClip.length);
      clipPlayable.SetSpeed(AttackSpeed);
      Slot.GetBehaviour().Play(clipPlayable);
      InputRouter.Instance.ConsumeButton("Attack", 0);
    }

    if (dashState == ButtonState.JustDown) {
      var clipPlayable = AnimationClipPlayable.Create(Graph, DiveRollAnimationClip);
      clipPlayable.SetApplyFootIK(true);
      clipPlayable.SetApplyPlayableIK(true);
      clipPlayable.SetDuration(DiveRollAnimationClip.length);
      clipPlayable.SetSpeed(DiveRollSpeed);
      Slot.GetBehaviour().Play(clipPlayable);
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
    SmoothLocomotionSpeed = Mathf.MoveTowards(SmoothLocomotionSpeed, LocomotionSpeed, LocalClock.DeltaTime() * SpeedChangeSpeed);

    // We can read inputs here to simulate attacking
    LocomotionSelect.GetBehaviour().CrossFade(Grounded ? 0 : 1, TransitionSpeed);
    GroundedSelect.GetBehaviour().CrossFade(SmoothLocomotionSpeed <= 0 ? 0 : 1, TransitionSpeed);
    if (SmoothLocomotionSpeed > 0) {
      var bestFittingGroundClipIndex = IndexWithBestSpeedMatch(GroundMovingClipPlayables, SmoothLocomotionSpeed);
      var clipPlayable = GroundMovingClipPlayables[bestFittingGroundClipIndex];
      var averageSpeed = clipPlayable.GetAnimationClip().averageSpeed.z;
      clipPlayable.SetSpeed(SmoothLocomotionSpeed / averageSpeed);
      GroundedMovingSelect.GetBehaviour().CrossFade(bestFittingGroundClipIndex, true, TransitionSpeed);
    }
    AirborneSelect.GetBehaviour().CrossFade(0, 1);
    Graph.Evaluate(LocalClock.DeltaTime());
  }

  void OnAnimatorMove() {
    var slot = Slot.GetBehaviour();
    if (slot.IsRunning && slot.ActivePlayable.HasValue) {
      var speed = 1f;
      if      (slot.ActivePlayable.Value.GetAnimationClip() == AttackAnimationClip) speed = AttackRootMotionScalar;
      else if (slot.ActivePlayable.Value.GetAnimationClip() == DiveRollAnimationClip) speed = DiveRollRootMotionScalar;
      transform.position += speed * Animator.deltaPosition;
    }
  }

  // Do not pass empty array unless you want to cry or check for -1 at call-site
  int IndexWithBestSpeedMatch(AnimationClipPlayable[] clipPlayables, float speed) {
    int bestIndex = -1;
    float lowestDistance = float.MaxValue;
    for (var i = 0; i < clipPlayables.Length; i++) {
      var clip = clipPlayables[i];
      var clipAverageSpeed = clip.GetAnimationClip().averageSpeed.z;
      var speedDistance = Mathf.Abs(clipAverageSpeed-speed);
      if (speedDistance < lowestDistance) {
        bestIndex = i;
        lowestDistance = speedDistance;
      }
    }
    return bestIndex;
  }
}