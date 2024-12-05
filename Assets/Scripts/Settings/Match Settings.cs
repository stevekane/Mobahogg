using UnityEngine;

[CreateAssetMenu(fileName = "MatchSettings", menuName = "Scriptable Objects/MatchSettings")]
public class MatchSettings : ScriptableObject {
  public int INITIAL_REQUIRED_RESOURCES = 21;
  public int INITIAL_LIVES = 9;
}