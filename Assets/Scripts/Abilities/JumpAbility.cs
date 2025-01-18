using UnityEngine;
using Abilities;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class JumpAbility : Ability {
  public override bool CanRun => true;
  public override bool CanStop => false;
  public override bool IsRunning { get; }
  public override void Run() {}
  public override void Stop() {}
}