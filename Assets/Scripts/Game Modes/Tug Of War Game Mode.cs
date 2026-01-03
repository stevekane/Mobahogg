using UnityEngine;

class TugOfWarGameMode : MonoBehaviour
{
  [SerializeField] Mouth TurtleMouth;
  [SerializeField] Mouth RobotMouth;

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
    TurtleMouth.Activate();
    RobotMouth.Activate();
  }
}