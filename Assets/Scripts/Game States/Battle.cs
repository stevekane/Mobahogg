using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000000)]
public class Battle : MonoBehaviour {
  public Timeval PreBattleDuration = Timeval.FromSeconds(3);
  public Timeval PostBattleDuration = Timeval.FromSeconds(3);
  public List<PotentialPlayer> Players = new() {
    new() { Name = "Alice", TeamType = TeamType.Turtles, State = PotentialPlayerState.Ready},
    new() { Name = "Bob", TeamType = TeamType.Turtles, State = PotentialPlayerState.Ready },
    new() { Name = "Jim", TeamType = TeamType.Robots, State = PotentialPlayerState.Ready },
    new() { Name = "Connie", TeamType = TeamType.Robots, State = PotentialPlayerState.Ready }
   };

void Start() {
    if (!MatchManager.Instance.IsActiveMatch) {
      var matchConfig = ScriptableObject.CreateInstance<MatchConfig>();
      matchConfig.BattleSceneNames = new string[1] { SceneManager.GetActiveScene().name };
      matchConfig.ForceReloadScene = false;
      matchConfig.RepeatMatch = true;
      matchConfig.StartingBattleIndex = 0;
      matchConfig.PreBattleDuration = PreBattleDuration;
      matchConfig.PostBattleDuration = PostBattleDuration;
      MatchManager.Instance.Players = Players;
      MatchManager.Instance.MatchConfig = matchConfig;
      MatchManager.Instance.IsActiveMatch = true;
    }
  }
}