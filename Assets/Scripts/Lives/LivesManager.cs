using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Mono.Cecil.Cil;
using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class LivesManager : MonoBehaviour {
  public static LivesManager Active;

  [SerializeField] int RespawnFrameDelay = 60 * 3;
  [SerializeField] List<Player> PlayerPrefabs;

  public LinkedList<RespawnPod> RespawnPods = new();
  public LinkedList<Player> Players = new();

  bool OnTeam(TeamType teamType, MonoBehaviour m) =>
    m.TryGetComponent(out Team team) && team.TeamType == teamType;
  IEnumerable<TeamType> Teams =>
    Enum.GetValues(typeof(TeamType)).Cast<TeamType>();
  RespawnPod UsablePodForTeam(TeamType teamType) =>
    RespawnPods.FirstOrDefault(p => p.Usable(teamType));
  Player PlayerPrefabForTeam(TeamType teamType) =>
    PlayerPrefabs.FirstOrDefault(p => OnTeam(teamType, p));

  // Called when player is fully dead after animations and such things
  public void OnPlayerDeath(Player victim) {
    var playerTeam = victim.GetComponent<Team>();
    var playerCreepOwner = victim.GetComponent<CreepOwner>();
    playerCreepOwner.Creeps.ForEach(c => c.State = DeadCreepState.Free);
    playerCreepOwner.Creeps.ForEach(c => c.Owner = null);
    Destroy(victim.gameObject);
    TryStartRespawnFromPod(playerTeam.TeamType, victim.PortIndex);
  }

  // Called to begin spawning process from a pod
  public void TryStartRespawnFromPod(TeamType teamType, int portIndex) {
    var pod = UsablePodForTeam(teamType);
    Debug.Log($"{pod} found for connected player {MatchManager.Instance.Players[portIndex].Name}on port {portIndex}");
    if (pod) {
      pod.StartRespawn(RespawnFrameDelay, portIndex);
    }
  }

  // Called when a pod should open to release a new player
  public Player SpawnPlayerFromPod(RespawnPod pod) {
    var team = pod.GetComponent<Team>();
    var position = pod.transform.position;
    var rotation = pod.transform.rotation;
    var prefab = PlayerPrefabForTeam(team.TeamType);
    var player = Instantiate(prefab, position, rotation, transform);
    player.PortIndex = pod.PortIndex;
    return player;
  }

  void Awake() {
    Active = this;
  }

  // TODO: Doing this OnStart feels kinda stupid.
  // Probably should be some explicit sort of "OnBattle" or "OnPreBattle"
  void Start() {
    Debug.Log($"{MatchManager.Instance.Players.Count} Players connected and should be spawned");
    MatchManager.Instance.Players.ForEach((p,i) => TryStartRespawnFromPod(p.TeamType, i));
  }

  void FixedUpdate() {
    foreach (var teamType in Teams) {
      var playersAlive = Players.Count(p => OnTeam(teamType, p));
      var podsAlive = RespawnPods.Count(p => OnTeam(teamType, p));
      MatchManager.Instance.SetLives(teamType, playersAlive + podsAlive);
    }
  }
}