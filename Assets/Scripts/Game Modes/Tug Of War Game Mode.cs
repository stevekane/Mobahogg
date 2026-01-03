using UnityEngine;

class TugOfWarGameMode : MonoBehaviour
{
  void Start()
  {
    MatchManager.Instance.OnBattleStart.Listen(OnBattleStart);
  }

  void OnDestroy()
  {
    MatchManager.Instance.OnBattleStart.Unlisten(OnBattleStart);
  }

  void OnBattleStart()
  {
    MatchManager.Instance.Players.ForEach(SpawnManager.Active.Spawn);
  }
}