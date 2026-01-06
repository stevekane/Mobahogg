using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000000)]
public class Battle : MonoBehaviour
{
  #if UNITY_EDITOR
  public MatchConfig MatchConfig;
  public List<PotentialPlayer> Players = new() {
    new() { Name = "Alice", TeamType = TeamType.Turtles, State = PotentialPlayerState.Ready},
    new() { Name = "Bob", TeamType = TeamType.Turtles, State = PotentialPlayerState.Ready },
    new() { Name = "Jim", TeamType = TeamType.Robots, State = PotentialPlayerState.Ready },
    new() { Name = "Connie", TeamType = TeamType.Robots, State = PotentialPlayerState.Ready }
  };

  void Start()
  {
    if (!MatchConfig)
    {
      Debug.LogError($"No match config on Battle. Needed to directly load this scene");
      return;
    }

    if (MatchManager.Instance.MatchConfig)
    {
      return;
    }

    var startingSceneIndex = MatchConfig.SceneReferences.IndexOf(sr => sr.SceneName == gameObject.scene.name);
    if (startingSceneIndex < 0)
    {
      Debug.LogError($"{gameObject.scene.name} is not found among scenes in provided match config");
      return;
    }

    var matchConfig = Instantiate(MatchConfig);
    matchConfig.StartingBattleIndex = startingSceneIndex;
    MatchManager.Instance.Players = Players;
    MatchManager.Instance.MatchConfig = matchConfig;
    MatchManager.Instance.StartMatch();
  }
  #endif
}