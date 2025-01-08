using Abilities;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class HoverAbility : MonoBehaviour, IAbility, Async {
  public bool IsRunning { get; set; }
  public bool CanRun => true;
  public void Run() {}
}