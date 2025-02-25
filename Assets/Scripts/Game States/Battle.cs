using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000000)]
public class Battle : MonoBehaviour {
  public Timeval PreBattleDuration = Timeval.FromSeconds(3);
  public Timeval PostBattleDuration = Timeval.FromSeconds(3);
  public List<PotentialPlayer> Players = new List<PotentialPlayer> {
    new PotentialPlayer { Name = "Alice", Team = true, State = PotentialPlayerState.Ready},
    new PotentialPlayer { Name = "Bob", Team = false, State = PotentialPlayerState.Ready },
    new PotentialPlayer { Name = "Jim", Team = true, State = PotentialPlayerState.Ready },
    new PotentialPlayer { Name = "Connie", Team = false, State = PotentialPlayerState.Ready }
   };

void Start() {
    // When we load up, let's determine if we're being played directly or
    if (!MatchManager.Instance.IsActiveMatch) {
      var matchConfig = ScriptableObject.CreateInstance<MatchConfig>();
      matchConfig.BattleSceneNames = new string[1] { SceneManager.GetActiveScene().name };
      matchConfig.ForceReloadScene = false;
      matchConfig.RepeatMatch = true;
      matchConfig.StartingBattleIndex = 0;
      matchConfig.PreBattleDuration = PreBattleDuration;
      matchConfig.PostBattleDuration = PostBattleDuration;
      MatchManager.Instance.StartMatch(Players, matchConfig);
    }
  }
}