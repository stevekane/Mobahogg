using Abilities;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class HoverAbility : Ability {
  public override bool CanRun => true;
  public override bool CanStop => false;
  public override bool IsRunning { get; }
  public override void Run() {}
  public override void Stop() {}
}