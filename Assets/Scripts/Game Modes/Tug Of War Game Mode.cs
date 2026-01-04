using UnityEngine;

class TugOfWarGameMode : MonoBehaviour
{
  [SerializeField] Mouth TurtleMouth;
  [SerializeField] Mouth RobotMouth;

  [ContextMenu("End")]
  void End()
  {
    MatchManager.Instance.EndBattle(Random.Range(-1, 1));
  }

  void Awake()
  {
    MatchManager.Instance.OnBattleStart.Listen(OnBattleStart);
    MatchManager.Instance.OnPreBattleStart.Listen(OnPreBattleStart);
    MatchManager.Instance.OnPostBattleStart.Listen(OnPostBattleStart);
  }

  void OnDestroy()
  {
    MatchManager.Instance.OnBattleStart.Unlisten(OnBattleStart);
    MatchManager.Instance.OnPreBattleStart.Unlisten(OnPreBattleStart);
    MatchManager.Instance.OnPostBattleStart.Unlisten(OnPostBattleStart);
  }

  void OnPreBattleStart()
  {
    TurtleMouth.gameObject.SetActive(false);
    RobotMouth.gameObject.SetActive(false);
  }

  void OnPostBattleStart()
  {
    TurtleMouth.gameObject.SetActive(false);
    RobotMouth.gameObject.SetActive(false);
  }

  void OnBattleStart()
  {
    MatchManager.Instance.Players.ForEach(SpawnManager.Active.Spawn);
    TurtleMouth.gameObject.SetActive(true);
    RobotMouth.gameObject.SetActive(true);
    TurtleMouth.Activate();
    RobotMouth.Activate();
  }
}