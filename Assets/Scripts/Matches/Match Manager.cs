using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

  public readonly EventSource OnBattleStart = new();
  public readonly EventSource OnBattleEnd = new();

  MatchState MatchState;

  public bool HasMatchConfig => MatchConfig != null;

  public void StartMatch()
  {
    Debug.Assert(HasMatchConfig, "Must have active Match Config");
    MatchState = new()
    {
      BattleIndex = MatchConfig.StartingBattleIndex
    };
    NextBattleOffset = 0;
    Timer = 0;
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
    var battleSceneName = MatchConfig.SceneName(battleIndex);
    var battleLoad = SceneManager.LoadSceneAsync(battleSceneName);
    battleLoad.completed += OnBattleLoad;
    MatchState.BattleState = BattleState.LoadBattle;
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
  }

  void OnBattleLoad(AsyncOperation battleLoad)
  {
    Debug.Assert(MatchState.BattleState == BattleState.LoadBattle, "OnBattleLoad invoked when not in load battle state");
    battleLoad.completed -= OnBattleLoad;
    StartPreBattle();
  }

  void StartPreBattle()
  {
    MatchState.BattleState = BattleState.PreBattle;
    Timer = MatchConfig.PreBattleDuration.Ticks;
    PreBattleOverlay.gameObject.SetActive(true);
    PostBattleOverlay.gameObject.SetActive(false);
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
    MatchState.BattleState = BattleState.PreBattle;
    Timer = MatchConfig.PreBattleDuration.Ticks;
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(true);
  }

  void EndMatch()
  {
    Debug.Log("Match is over");
    MatchState.BattleState = BattleState.NoBattle;
    MatchState.Complete = true;
    PreBattleOverlay.gameObject.SetActive(false);
    PostBattleOverlay.gameObject.SetActive(false);
  }

  void FixedUpdate()
  {
    if (MatchState == null)
      return;

    if (MatchState.BattleState == BattleState.PreBattle)
    {
      if (Timer <= 0)
      {
        StartActiveBattle();
      }
      else
      {
        Timer--;
        PreBattleOverlay.SetCountdown((float)Timer/60);
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