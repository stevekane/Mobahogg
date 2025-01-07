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

  [Header("Battle State")]
  public List<TeamState> Teams;

  [Header("References")]
  [SerializeField] PreBattleOverlay PreBattleOverlay;
  [SerializeField] PostBattleOverlay PostBattleOverlay;

  int MaxBattleIndex =>
    MatchConfig
      ? MatchConfig.BattleSceneNames.Length / 2
      : 0;
  string CurrentBattleScreen =>
    MatchConfig
      ? MatchConfig.SceneName(BattleIndex + MatchConfig.BattleCount / 2)
      : null;

  // Mactch configuration
  IEnumerable<PotentialPlayer> PotentialPlayers;
  MatchConfig MatchConfig;

  // TODO: This is really hacky way to "check for this". This whole class
  // should get rewritten to be more evented and far less stupid than it currently is.
  public bool IsActiveMatch => MatchConfig != null;

  EventSource OnStartMatch = new();

  public void SetLives(TeamType teamType, int count) {
    Teams.Where(t => t.HasType(teamType)).ForEach(t => t.LivesRemaining = count);
  }

  public void SetRequiredResources(TeamType teamType, int count) {
    Teams.Where(t => t.HasType(teamType)).ForEach(t => t.ResourcesRequired = count);
  }

  public void DeductRequiredResource(TeamType teamType) {
    Teams.Where(t => t.HasType(teamType)).ForEach(t => t.ResourcesRequired--);
  }

  public void DefeatByGolem(TeamType teamType) {
    Teams.Where(t => t.HasType(teamType)).ForEach(t => t.KilledByGolem = true);
  }

  public void StartMatch(IEnumerable<PotentialPlayer> potentialPlayers, MatchConfig matchConfig) {
    PotentialPlayers = potentialPlayers;
    MatchConfig = matchConfig;
    BattleIndex = matchConfig.StartingBattleIndex;
    Players = PotentialPlayers.Select(ActivePlayer.From).ToList();
    OnStartMatch.Fire();
  }

  // TODO: This throws an error when application shuts down. Not sure it matters
  // but perhaps it indicates a subtle issue in understanding.
  protected override void AwakeSingleton() {
    Messages(this.destroyCancellationToken).Forget();
  }

  async UniTask Messages(CancellationToken token) {
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
    CancellationTokenSource matchTokenSource = null;
    try {
      while (true) {
        await Tasks.ListenFor(OnStartMatch, token);
        matchTokenSource?.Cancel();
        matchTokenSource?.Dispose();
        matchTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        RunMatch(matchTokenSource.Token).Forget();
      }
    } finally {
      matchTokenSource?.Cancel();
      matchTokenSource?.Dispose();
    }
  }

  async UniTask<MatchResult> RunMatch(CancellationToken token) {
    BattleResult battleResult = null;
    MatchResult matchResult = null;
    await PreMatch(token);
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

  async UniTask PreMatch(CancellationToken token) {
    await UniTask.NextFrame(token);
  }

  async UniTask PostMatch(MatchResult result, CancellationToken token) {
    await UniTask.NextFrame(token);
    // Kind of a hacky way to try to restart the match for quick testing...
    if (MatchConfig.RepeatMatch) {
      Debug.Log("Calling Start match for repeat reasons");
      StartMatch(PotentialPlayers, MatchConfig);
    }
  }

  // TODO: Check conditions sync and end immediately otherwise wait till next frame?
  async UniTask<BattleResult> RunBattle(CancellationToken token) {
    BattleResult battleResult = null;
    await PreBattle(token);
    Teams = new() { new TeamState(TeamType.Turtles), new TeamState(TeamType.Robots) };
    do {
      await UniTask.NextFrame(cancellationToken: token);
      battleResult = CheckBattleWinningConditions();
    } while (battleResult == null);
    await PostBattle(battleResult, token);
    return battleResult;
  }

  async UniTask PreBattle(CancellationToken token) {
    try {
      await SceneManager.LoadSceneAsync(CurrentBattleScreen);
      PreBattleOverlay.gameObject.SetActive(true);
      PreBattleOverlay.SetName(CurrentBattleScreen);
      PreBattleOverlay.SetBattleIndex(BattleIndex, max: MatchConfig.BattleCount / 2);
      PreBattleOverlay.SetCountdown(Mathf.RoundToInt(MatchConfig.PreBattleDuration.Seconds));
      var duration = MatchConfig.PreBattleDuration.Seconds;
      void UpdateCountdown(float dt, float elapsed) => PreBattleOverlay.SetCountdown(duration-elapsed);
      await Tasks.DoEveryFrameForDuration(duration, UpdateCountdown, token);
    } finally {
      PreBattleOverlay.gameObject.SetActive(false);
    }
  }

  async UniTask PostBattle(BattleResult result, CancellationToken token) {
    try {
      PostBattleOverlay.gameObject.SetActive(true);
      PostBattleOverlay.SetWinner(result.ToString());
      await UniTask.DelayFrame(MatchConfig.PostBattleDuration.Ticks, delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);
    } finally {
      PostBattleOverlay.gameObject.SetActive(false);
    }
  }

  // TODO: Make use of this allocated list to avoid allocations during winning conditions checks
  List<BattleResult> ScreenResults = new(4);
  BattleResult CheckBattleWinningConditions() {
    var killerResult = ResultFor(t => t.LivesRemaining <= 0, t => t.LivesRemaining > 0, VictoryType.Killer);
    var summonerResult = ResultFor(t => t.KilledByGolem, t => !t.KilledByGolem, VictoryType.Summoner);
    var foragerResult = ResultFor(t => t.ResourcesRequired <= 0, t => t.ResourcesRequired <= 0, VictoryType.Forager);
    var results = new List<BattleResult> { killerResult, summonerResult, foragerResult };
    var result = results.FirstOrDefault(r => r.Teams.Length == 1) ?? results.FirstOrDefault(r => r.Teams.Length > 1);
    return result;
  }

  // TODO: Make use of this allocated list to avoid allocations during winning conditions checks
  List<TeamState> WinningConditionsTeams = new(4);
  BattleResult ResultFor(Func<TeamState,bool> endingCondition, Func<TeamState, bool> winningCondition, VictoryType victoryType) {
    return new BattleResult(victoryType, Teams.Any(endingCondition) ? Teams.Where(winningCondition).ToArray() : new TeamState[0]);
  }

  MatchResult CheckMatchWinningConditions() {
    return BattleIndex <= -MaxBattleIndex
      ? new MatchResult { WinningTeamType = TeamType.Turtles }
      : BattleIndex >= MaxBattleIndex
        ? new MatchResult { WinningTeamType = TeamType.Robots }
        : null;
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
  public TeamState[] Teams;
  public TeamType? Winner => Teams.Length == 1 ? Teams[0].TeamType : null;
  public BattleResult(VictoryType victoryType, TeamState[] teams) {
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
  public bool IsMemberOf(TeamState team) => TeamType == team.TeamType;
  public static ActivePlayer From(PotentialPlayer potentialPlayer) {
    return new ActivePlayer {
      TeamType = potentialPlayer.Team ? TeamType.Turtles : TeamType.Robots,
      Name = potentialPlayer.Name,
    };
  }
}

[Serializable]
public class TeamState {
  public TeamType TeamType;
  public int LivesRemaining;
  public int ResourcesRequired;
  public bool KilledByGolem;
  public bool HasType(TeamType type) => TeamType == type;
  public bool HasMember(ActivePlayer player) => TeamType == player.TeamType;
  public TeamState(TeamType teamType) {
    TeamType = teamType;
    KilledByGolem = false;
    LivesRemaining = MatchManager.Instance.MatchSettings.INITIAL_LIVES;
    ResourcesRequired = MatchManager.Instance.MatchSettings.INITIAL_REQUIRED_RESOURCES;
  }
  public void Reset() {
    KilledByGolem = false;
    LivesRemaining = MatchManager.Instance.MatchSettings.INITIAL_LIVES;
    ResourcesRequired = MatchManager.Instance.MatchSettings.INITIAL_REQUIRED_RESOURCES;
  }
}