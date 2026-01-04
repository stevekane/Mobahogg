using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class SpawnManager : MonoBehaviour {
  public static SpawnManager Active;

  [SerializeField] List<Player> PlayerPrefabs;

  public List<PlayerSpawn> PlayerSpawns = new();
  public List<Player> Players = new();

  public readonly EventSource<Player> OnAddPlayer = new();
  public readonly EventSource<Player> OnRemovePlayer = new();

  bool OnTeam(TeamType teamType, MonoBehaviour m) =>
    m.TryGetComponent(out Team team) && team.TeamType == teamType;
  Player PlayerPrefabForTeam(TeamType teamType) =>
    PlayerPrefabs.FirstOrDefault(p => OnTeam(teamType, p));

  public void Add(PlayerSpawn spawn) => PlayerSpawns.Add(spawn);
  public void Remove(PlayerSpawn spawn) => PlayerSpawns.Remove(spawn);

  public void Add(Player player) {
    Players.Add(player);
    OnAddPlayer.Fire(player);
  }

  public void Remove(Player player) {
    Players.Remove(player);
    OnRemovePlayer.Fire(player);
  }

  public void Respawn(Player victim) {
    var potentialPlayer = MatchManager.Instance.Players[victim.PortIndex];
    Destroy(victim.gameObject);
    Spawn(potentialPlayer, victim.PortIndex);
  }

  public void Spawn(PotentialPlayer potentialPlayer, int portIndex) {
    var spawn = PlayerSpawns.First(s => OnTeam(potentialPlayer.TeamType, s));
    var prefab = PlayerPrefabForTeam(potentialPlayer.TeamType);
    spawn.transform.GetPositionAndRotation(out var position, out var rotation);
    var player = Instantiate(prefab, position, rotation, transform);
    player.PortIndex = portIndex;
  }

  void Awake() {
    Active = this;
  }
}