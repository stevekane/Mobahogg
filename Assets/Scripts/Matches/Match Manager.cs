using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class MatchManager : SingletonBehavior<MatchManager>
{
  [Header("References")]
  [SerializeField] LoadBattleOverlay LoadBattleOverlay;
  [SerializeField] PreBattleOverlay PreBattleOverlay;
  [SerializeField] PostBattleOverlay PostBattleOverlay;

  string WinnerText(int nextBattleIndex) => nextBattleIndex switch
  {
    > 0 => "Turtles Win",
    < 0 => "Robots Win",
    _ => "Tie"
  };

  public MatchConfig MatchConfig;
  public List<PotentialPlayer> Players;
  public int Timer;
  public int NextBattleOffset;

  public readonly EventSource OnPreBattleStart = new();
  public readonly EventSource OnPostBattleStart = new();
  public readonly EventSource OnBattleStart = new();
  public readonly EventSource OnBattleEnd = new();

  MatchState MatchState;
  AsyncOperation LoadBattleOperation;

  public void StartMatch()
  {
    Debug.Assert(MatchConfig != null, "Must have active Match Config");
    MatchState = new()
    {
      BattleIndex = MatchConfig.StartingBattleIndex
    };
    Timer = 0;
    NextBattleOffset = 0;
    LoadBattle(MatchState.BattleIndex);
  }

  public void EndBattle(int nextBattleOffset)
  {
    NextBattleOffset = nextBattleOffset;
    StartPostBattle();
    OnBattleEnd.Fire();
  }

  void LoadBattle(int battleIndex)
  {
    var loadingBattleScene = SceneManager.GetSceneByName(MatchConfig.SceneName(battleIndex));
    LoadBattleOperation = SceneManager.LoadSceneAsync(loadingBattleScene.name);
    MatchState.BattleState = BattleState.LoadBattle;
    LoadBattleOverlay.gameObject.SetActive(true);
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
  }

  void StartPreBattle()
  {
    MatchState.BattleState = BattleState.PreBattle;
    Timer = MatchConfig.PreBattleDuration.Ticks;
    LoadBattleOverlay.gameObject.SetActive(false);
    PreBattleOverlay.gameObject.SetActive(true);
    PreBattleOverlay.SetBattleIndex(MatchState.BattleIndex, MatchConfig.BattleCount);
    PreBattleOverlay.SetName(MatchConfig.SceneName(MatchState.BattleIndex));
    PostBattleOverlay.gameObject.SetActive(false);
    OnPreBattleStart.Fire();
  }

  void StartActiveBattle()
  {
    MatchState.BattleState = BattleState.ActiveBattle;
    LoadBattleOverlay.gameObject.SetActive(false);
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
    OnBattleStart.Fire();
  }

  void StartPostBattle()
  {
    MatchState.BattleState = BattleState.PostBattle;
    Timer = MatchConfig.PostBattleDuration.Ticks;
    LoadBattleOverlay.gameObject.SetActive(false);
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(true);
    PostBattleOverlay.SetWinner(WinnerText(NextBattleOffset));
    OnPostBattleStart.Fire();
  }

  void EndMatch()
  {
    MatchState.BattleState = BattleState.NoBattle;
    MatchState.Complete = true;
    LoadBattleOverlay.gameObject.SetActive(false);
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
  }

  void FixedUpdate()
  {
    if (MatchState == null)
      return;

    if (MatchState.BattleState == BattleState.LoadBattle)
    {
      LoadBattleOverlay.SetCompletionFraction(LoadBattleOperation.progress);
      if (LoadBattleOperation.isDone)
      {
        StartPreBattle();
      }
    }
    else if (MatchState.BattleState == BattleState.PreBattle)
    {
      if (Timer <= 0)
      {
        StartActiveBattle();
      }
      else
      {
        Timer--;
        PreBattleOverlay.SetCountdown((float)Timer / 60);
      }
    }
    else if (MatchState.BattleState == BattleState.PostBattle)
    {
      if (Timer <= 0)
      {
        MatchState.BattleIndex += NextBattleOffset;
        if (MatchState.BattleIndex >= 0 && MatchState.BattleIndex < MatchConfig.BattleCount)
        {
          LoadBattle(MatchState.BattleIndex);
        }
        else
        {
          // Eventually there should be proper end match handling
          // For now, we just start it again
          // EndMatch();
          StartMatch();
        }
      }
      else
      {
        Timer--;
      }
    }
  }
}

enum BattleState
{
  NoBattle,
  LoadBattle,
  PreBattle,
  ActiveBattle,
  PostBattle
}

class MatchState
{
  public bool Complete;
  public int BattleIndex;
  public BattleState BattleState;
}