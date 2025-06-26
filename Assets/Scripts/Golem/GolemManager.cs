using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class GolemManager : MonoBehaviour {
  public static GolemManager Active;

  public List<Golem> Golems = new();
  public List<GolemAttractor> GolemAttractors = new();
}