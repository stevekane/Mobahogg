using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum PotentialPlayerState {
  Disconnected,
  Connected,
  Joined,
  Ready
}

public class PotentialPlayer {
  public PotentialPlayerState State;
  public string Name = "Player";
  public bool Team;
}

public static class Names {
  public static string[] Value = new string[8] {
    "Davis",
    "Jenny",
    "Polter",
    "Wanda",
    "Crassus",
    "Magpie",
    "Virgin",
    "Galaga",
  };

  public static string GetRandom => Value[Random.Range(0, Value.Length)];
}

public class MatchSetup : MonoBehaviour {
  [SerializeField] List<MatchSetupPlayerGridCard> PlayerGridCards;

  List<PotentialPlayer> Players;

  void Start() {
    Players = new(InputRouter.MAX_PORT_COUNT);
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      var player = new PotentialPlayer { Name = Names.GetRandom };
      Players.Add(player);
      PlayerGridCards[i].Render(player);
    }
    InputRouter.Instance.DeviceToPortMap.Values.ForEach(AddDevice);
    InputRouter.Instance.DeviceConnected.Listen(AddDevice);
    InputRouter.Instance.DeviceConnected.Listen(RemoveDevice);
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryListen("MatchSetup/ToggleJoin", i, HandleToggleJoin);
      InputRouter.Instance.TryListen("MatchSetup/ToggleTeam", i, HandleToggleTeam);
      InputRouter.Instance.TryListen("MatchSetup/ToggleReady", i, HandleToggleReady);
      InputRouter.Instance.TryListen("MatchSetup/ChangeName", i, HandleChangeName);
    }
  }

  void OnDestroy() {
    InputRouter.Instance.DeviceConnected.Unlisten(AddDevice);
    InputRouter.Instance.DeviceDisconnected.Unlisten(RemoveDevice);
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryUnlisten("MatchSetup/ToggleJoin", i, HandleToggleJoin);
      InputRouter.Instance.TryUnlisten("MatchSetup/ToggleTeam", i, HandleToggleTeam);
      InputRouter.Instance.TryUnlisten("MatchSetup/ToggleReady", i, HandleToggleReady);
      InputRouter.Instance.TryUnlisten("MatchSetup/ChangeName", i, HandleChangeName);
    }
  }

  void HandleToggleJoin(PortAction action) {
    ToggleJoin(action.PortIndex);
  }

  void HandleToggleTeam(PortAction action) {
    ToggleTeam(action.PortIndex);
  }

  void HandleChangeName(PortAction action) {
    ChangeName(action.PortIndex);
  }

  void HandleToggleReady(PortAction action) {
    ToggleReady(action.PortIndex);
  }

  void AddDevice(int index) {
    Players[index].State = PotentialPlayerState.Connected;
    PlayerGridCards[index].Render(Players[index]);
  }

  void RemoveDevice(int index) {
    Players[index].State = PotentialPlayerState.Disconnected;
    PlayerGridCards[index].Render(Players[index]);
  }

  void ToggleJoin(int index) {
    var player = Players[index];
    player.State = player.State switch {
      PotentialPlayerState.Joined => PotentialPlayerState.Connected,
      PotentialPlayerState.Connected => PotentialPlayerState.Joined,
      _ => player.State
    };
    PlayerGridCards[index].Render(player);
  }

  void ToggleTeam(int index) {
    var player = Players[index];
    player.Team = !player.Team;
    PlayerGridCards[index].Render(player);
  }

  void ToggleReady(int index) {
    var player = Players[index];
    player.State = player.State switch {
      PotentialPlayerState.Ready => PotentialPlayerState.Joined,
      PotentialPlayerState.Joined => PotentialPlayerState.Ready,
      _ => player.State
    };
    PlayerGridCards[index].Render(player);
    if (Players.TrueForAll(p => p.State == PotentialPlayerState.Ready || p.State == PotentialPlayerState.Disconnected)) {
      MatchManager.Instance.StartMatch(Players.Where(p => p.State == PotentialPlayerState.Ready));
    }
  }

  void ChangeName(int index) {
    var player = Players[index];
    var currentName = player.Name;
    do {
      player.Name = Names.GetRandom;
    } while (player.Name == currentName);
    PlayerGridCards[index].Render(player);
  }
}