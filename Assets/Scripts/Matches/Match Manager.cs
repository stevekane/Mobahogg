using System.Collections.Generic;
using UnityEngine;

public class MatchManager : SingletonBehavior<MatchManager> {
  public MatchSettings MatchSettings;
  public MatchConfig MatchConfig;
  public List<PotentialPlayer> Players;
  public bool IsActiveMatch = false;

  [Header("References")]
  [SerializeField] PreBattleOverlay PreBattleOverlay;
  [SerializeField] PostBattleOverlay PostBattleOverlay;
}