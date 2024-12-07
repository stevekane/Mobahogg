using System;
using System.Collections.Generic;
using System.Linq;
using State;
using UnityEngine;

public class LivesManager : MonoBehaviour {
  public static LivesManager Active;

  [SerializeField] int RespawnFrameDelay = 60 * 3;

  public LinkedList<RespawnPod> RespawnPods = new();
  public LinkedList<Player> Players = new();

  public void OnPlayerDeath(Player victim) {
    var playerTeam = victim.GetComponent<Team>();
    var playerCreepOwner = victim.GetComponent<CreepOwner>();
    var pod = RespawnPods.FirstOrDefault(p => p.Usable(playerTeam.TeamType));
    playerCreepOwner.Creeps.ForEach(c => c.Owner = null);
    Destroy(victim.gameObject);
    if (pod) {
      pod.Respawn(RespawnFrameDelay);
    }
  }

  void Awake() {
    Active = this;
  }

  /*
  NOTE:

  I am deciding for now to update the lives here through this idiotic polling method.
  This could be refactored various ways including using a more evented model like
  listening for "on egg death" and "on player death" and deducting more sparsely.
  */
  bool OnTeam(TeamType teamType, MonoBehaviour m) => m.TryGetComponent(out Team team) && team.TeamType == teamType;
  void FixedUpdate() {
    foreach (var teamType in Enum.GetValues(typeof(TeamType)).Cast<TeamType>()) {
      var playersAlive = Players.Count(p => OnTeam(teamType, p));
      var podsAlive = RespawnPods.Count(p => OnTeam(teamType, p));
      MatchManager.Instance.SetLives(teamType, playersAlive + podsAlive);
    }
  }
}