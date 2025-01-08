using UnityEngine;
using Abilities;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class JumpAbility : MonoBehaviour, IAbility {
  public bool CanRun => true;
  public void Run() {}
}