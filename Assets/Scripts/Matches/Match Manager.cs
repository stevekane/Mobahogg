using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : SingletonBehavior<MatchManager> {
  [Header("Settings")]
  public MatchSettings MatchSettings;

  [Header("Match State")]
  public int BattleIndex;
  public List<ActivePlayer> Players;
  public List<Team> Teams;

  [Header("Configuration")]
  [SerializeField] SceneAsset[] BattleScreens;
  [SerializeField] Timeval PregameDuration = Timeval.FromSeconds(3);

  [Header("References")]
  [SerializeField] PreBattleOverlay PreBattleOverlay;
  [SerializeField] PostBattleOverlay PostBattleOverlay;

  SceneAsset CurrentBattleScreen => BattleScreens[BattleIndex + BattleScreens.Length / 2];

  EventSource<IEnumerable<PotentialPlayer>> OnStartMatch = new();

  public void DeductLife(TeamType teamType) {
    Teams.Where(t => t.HasType(teamType)).ForEach(t => t.LivesRemaining--);
  }

  public void DeductRequiredResource(TeamType teamType) {
    Teams.Where(t => t.HasType(teamType)).ForEach(t => t.ResourcesRequired--);
  }

  public void StartMatch(IEnumerable<PotentialPlayer> potentialPlayers) {
    OnStartMatch.Fire(potentialPlayers);
  }

  protected override async void AwakeSingleton() {
    await Messages(this.destroyCancellationToken);
  }

  async UniTask Messages(CancellationToken token) {
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
    CancellationTokenSource matchTokenSource = null;
    try {
      while (true) {
        var potentialPlayers = await Tasks.ListenFor(OnStartMatch, token);
        matchTokenSource?.Cancel();
        matchTokenSource?.Dispose();
        matchTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        RunMatch(potentialPlayers, matchTokenSource.Token).Forget();
      }
    } finally {
      matchTokenSource?.Cancel();
      matchTokenSource?.Dispose();
    }
  }

  async UniTask<MatchResult> RunMatch(IEnumerable<PotentialPlayer> potentialPlayers, CancellationToken token) {
    BattleResult battleResult = null;
    MatchResult matchResult = null;
    await PreMatch(potentialPlayers, token);
    do {
      battleResult = await RunBattle(token);
      BattleIndex = battleResult.Winner switch {
        TeamType.Robots => BattleIndex+1,
        TeamType.Turtles => BattleIndex-1,
        _ => BattleIndex
      };
      matchResult = CheckMatchWinningConditions();
    } while (matchResult == null);
    await PostMatch(matchResult, token);
    return matchResult;
  }

  async UniTask PreMatch(IEnumerable<PotentialPlayer> potentialPlayers, CancellationToken token) {
    Debug.Log("Pre Match");
    Players = potentialPlayers.Select(ActivePlayer.From).ToList();
    await UniTask.NextFrame(token);
  }

  async UniTask PostMatch(MatchResult result, CancellationToken token) {
    Debug.Log($"Post Match {result}");
    await UniTask.NextFrame(token);
  }

  // TODO: Check conditions sync and end immediately otherwise wait till next frame?
  async UniTask<BattleResult> RunBattle(CancellationToken token) {
    BattleResult battleResult = null;
    await PreBattle(token);
    Teams = new() { new Team(TeamType.Turtles), new Team(TeamType.Robots) };
    do {
      await UniTask.NextFrame(cancellationToken: token);
      battleResult = CheckBattleWinningConditions();
    } while (battleResult == null);
    await PostBattle(battleResult, token);
    return battleResult;
  }

  async UniTask PreBattle(CancellationToken token) {
    try {
      Debug.Log("Pre Battle");
      SceneManager.LoadScene(CurrentBattleScreen.name);
      PreBattleOverlay.gameObject.SetActive(true);
      PreBattleOverlay.SetName(CurrentBattleScreen.name);
      PreBattleOverlay.SetBattleIndex(BattleIndex, max: 2);
      PreBattleOverlay.SetCountdown(Mathf.RoundToInt(PregameDuration.Seconds));
      var duration = PregameDuration.Seconds;
      void UpdateCountdown(float dt, float elapsed) => PreBattleOverlay.SetCountdown(duration-elapsed);
      await Tasks.DoEveryFrameForDuration(duration, UpdateCountdown, token);
    } finally {
      PreBattleOverlay.gameObject.SetActive(false);
    }
  }

  async UniTask PostBattle(BattleResult result, CancellationToken token) {
    try {
      Debug.Log($"Post Battle {result}");
      PostBattleOverlay.gameObject.SetActive(true);
      PostBattleOverlay.SetWinner(result.ToString());
      await UniTask.Delay(3000);
    } finally {
      PostBattleOverlay.gameObject.SetActive(false);
    }
  }

  // TODO: Make use of this allocated list to avoid allocations during winning conditions checks
  List<BattleResult> ScreenResults = new(4);
  BattleResult CheckBattleWinningConditions() {
    var killerResult = ResultFor(t => t.LivesRemaining <= 0, t => t.LivesRemaining > 0, VictoryType.Killer);
    var summonerResult = ResultFor(t => false, t => true, VictoryType.Summoner);
    var foragerResult = ResultFor(t => t.ResourcesRequired <= 0, t => t.ResourcesRequired <= 0, VictoryType.Forager);
    var results = new List<BattleResult> { killerResult, summonerResult, foragerResult };
    var result = results.FirstOrDefault(r => r.Teams.Length == 1) ?? results.FirstOrDefault(r => r.Teams.Length > 1);
    return result;
  }

  // TODO: Make use of this allocated list to avoid allocations during winning conditions checks
  List<Team> WinningConditionsTeams = new(4);
  BattleResult ResultFor(Func<Team,bool> endingCondition, Func<Team, bool> winningCondition, VictoryType victoryType) {
    return new BattleResult(victoryType, Teams.Any(endingCondition) ? Teams.Where(winningCondition).ToArray() : new Team[0]);
  }

  MatchResult CheckMatchWinningConditions() {
    return BattleIndex switch {
      < -2 => new MatchResult { WinningTeamType = TeamType.Turtles },
      > 2 => new MatchResult { WinningTeamType = TeamType.Robots },
      _ => null
    };
  }
}

// MATCH STATE
public enum TeamType {
  Turtles,
  Robots
}

public enum VictoryType {
  Killer,
  Summoner,
  Forager
}

[Serializable]
public class MatchResult {
  public TeamType WinningTeamType;
  public TeamType Winner => WinningTeamType;
  public override string ToString() {
    return $"{Winner} WINS";
  }
}

[Serializable]
public class BattleResult {
  public VictoryType VictoryType;
  public Team[] Teams;
  public TeamType? Winner => Teams.Length == 1 ? Teams[0].TeamType : null;
  public BattleResult(VictoryType victoryType, Team[] teams) {
    VictoryType = victoryType;
    Teams = teams;
  }
  public string WinningTeam => Teams.Length == 1 ? Teams[0].TeamType.ToString() : "";
  public string Outcome => Teams.Length switch {
    0 => "No Result",
    1 => "Win",
    _ => "Draw"
  };
  public override string ToString() {
    return Teams.Length switch {
      0 => $"No result by {VictoryType}",
      1 => $"{Winner} WINS by {VictoryType}",
      _ => $"Draw by {VictoryType}",
    };
  }
}

[Serializable]
public class ActivePlayer {
  public TeamType TeamType;
  public string Name;
  public bool IsMemberOf(Team team) => TeamType == team.TeamType;
  public static ActivePlayer From(PotentialPlayer potentialPlayer) {
    return new ActivePlayer {
      TeamType = potentialPlayer.Team ? TeamType.Turtles : TeamType.Robots,
      Name = potentialPlayer.Name,
    };
  }
}

[Serializable]
public class Team {
  public TeamType TeamType;
  public int LivesRemaining;
  public int ResourcesRequired;
  public bool HasType(TeamType type) => TeamType == type;
  public bool HasMember(ActivePlayer player) => TeamType == player.TeamType;
  public Team(TeamType teamType) {
    TeamType = teamType;
    LivesRemaining = MatchManager.Instance.MatchSettings.INITIAL_LIVES;
    ResourcesRequired = MatchManager.Instance.MatchSettings.INITIAL_REQUIRED_RESOURCES;
  }
  public void Reset() {
    LivesRemaining = MatchManager.Instance.MatchSettings.INITIAL_LIVES;
    ResourcesRequired = MatchManager.Instance.MatchSettings.INITIAL_REQUIRED_RESOURCES;
  }
}