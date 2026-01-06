using UnityEngine;

[CreateAssetMenu(fileName = "Match Config", menuName = "Matches/Match Config")]
public class MatchConfig : ScriptableObject {
  public Timeval PreBattleDuration = Timeval.FromSeconds(3);
  public Timeval PostBattleDuration = Timeval.FromSeconds(3);
  public SceneReference[] SceneReferences;
  public int StartingBattleIndex = 0;
  public int BattleCount => SceneReferences.Length;
  public string SceneName(int i) => SceneReferences[i].SceneName;
}