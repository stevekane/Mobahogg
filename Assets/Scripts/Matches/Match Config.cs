using UnityEngine;

[CreateAssetMenu(fileName = "MatchConfig", menuName = "Scriptable Objects/MatchConfig")]
public class MatchConfig : ScriptableObject {
  public bool ForceReloadScene = true;
  public bool RepeatMatch = false;
  public int StartingBattleIndex = 0;
  public Timeval PreBattleDuration = Timeval.FromSeconds(3);
  public Timeval PostBattleDuration = Timeval.FromSeconds(3);

  // TODO: This indirection is created here so that I can update
  // The way scene references are stored here but not throughout
  // the code that might refer to them
  public string[] BattleSceneNames;
  public int BattleCount => BattleSceneNames.Length;
  public string SceneName(int i) => BattleSceneNames[i];
}