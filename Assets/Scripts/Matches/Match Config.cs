using UnityEngine;

[CreateAssetMenu(fileName = "Match Config", menuName = "Matches/Match Config")]
public class MatchConfig : ScriptableObject {
  public bool ForceReloadScene = true;
  public bool RepeatMatch = false;
  public int StartingBattleIndex = 0;
  public Timeval PreBattleDuration = Timeval.FromSeconds(3);
  public Timeval PostBattleDuration = Timeval.FromSeconds(3);
  public string[] BattleSceneNames;
  public int BattleCount => BattleSceneNames.Length;
  public string SceneName(int i) => BattleSceneNames[i];
}