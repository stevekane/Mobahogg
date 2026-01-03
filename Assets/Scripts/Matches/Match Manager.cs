using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class MatchManager : SingletonBehavior<MatchManager>
{
  [Header("References")]
  [SerializeField] PreBattleOverlay PreBattleOverlay;
  [SerializeField] PostBattleOverlay PostBattleOverlay;
  [SerializeField] MatchSettings MatchSettings;

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

  public bool HasMatchConfig => MatchConfig != null;

  public void StartMatch()
  {
    Debug.Assert(HasMatchConfig, "Must have active Match Config");
    MatchState = new()
    {
      BattleIndex = MatchConfig.StartingBattleIndex
    };
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
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
  }

  void StartPreBattle()
  {
    MatchState.BattleState = BattleState.PreBattle;
    Timer = MatchConfig.PreBattleDuration.Ticks;
    PreBattleOverlay.gameObject.SetActive(true);
    PostBattleOverlay.gameObject.SetActive(false);
    OnPreBattleStart.Fire();
  }

  void StartActiveBattle()
  {
    MatchState.BattleState = BattleState.ActiveBattle;
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
    OnBattleStart.Fire();
  }

  void StartPostBattle()
  {
    MatchState.BattleState = BattleState.PostBattle;
    Timer = MatchConfig.PostBattleDuration.Ticks;
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(true);
    OnPostBattleStart.Fire();
  }

  void EndMatch()
  {
    MatchState.BattleState = BattleState.NoBattle;
    MatchState.Complete = true;
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
  }

  void FixedUpdate()
  {
    if (MatchState == null)
      return;

    if (MatchState.BattleState == BattleState.LoadBattle)
    {
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
          EndMatch();
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