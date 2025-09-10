using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HookshotStateMachine : MonoBehaviour
{
  public enum State
  {
    Ready,
    Windup,
    Traveling,
    Attached,
    Retracting,
    Pulling,
    Recovering
  }

  [SerializeField] string ActionName = "Jump";

  public State CurrentState;

  void FixedUpdate()
  {
    Action update = CurrentState switch
    {
      State.Ready => UpdateReady,
      State.Windup => UpdateWindup,
      State.Traveling => UpdateTraveling,
      State.Attached => UpdateAttached,
      State.Retracting => UpdateRetracting,
      State.Pulling => UpdatePulling,
      State.Recovering => UpdateRecovering,
    };
    update();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  static bool WaitThen(ref int frame, Timeval duration, Action action)
  {
    frame = Mathf.Max(duration.Ticks, frame + 1);
    if (frame >= duration.Ticks)
    {
      action();
      return true;
    }
    else
    {
      return false;
    }
  }

  void UpdateReady()
  {
    if (InputRouter.Instance.JustDownWithin(ActionName, 0))
    {
      Windup();
      InputRouter.Instance.ConsumeButton(ActionName, 0);
    }
  }

  [SerializeField] Timeval WindupDuration = Timeval.FromSeconds(0.15f);
  int WindupFrames;
  void Windup() {
    WindupFrames = 0;
    CurrentState = State.Windup;
  }

  void UpdateWindup()
  {
    WaitThen(ref WindupFrames, WindupDuration, Travel);
  }


  [SerializeField] Timeval TravelDuration = Timeval.FromSeconds(1f);
  int TravelFrames;
  void Travel()
  {
    TravelFrames = 0;
    CurrentState = State.Traveling;
  }

  void UpdateTraveling()
  {
    WaitThen(ref TravelFrames, TravelDuration, Attach);
  }

  [SerializeField] Timeval AttachDuration = Timeval.FromSeconds(.15f);
  int AttachedFrames;
  void Attach()
  {
    AttachedFrames = 0;
    CurrentState = State.Attached;
  }

  void UpdateAttached()
  {
    WaitThen(ref AttachedFrames, AttachDuration, Pull);
  }

  void UpdatePulling()
  {

  }

  void UpdateRetracting()
  {

  }

  void UpdateRecovering()
  {

  }

  void Pull()
  {

  }

  void Recover()
  {

  }

}