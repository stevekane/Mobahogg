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
  [SerializeField] MatchConfig MatchConfig;

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
      InputRouter.Instance.TryListenButton("MatchSetup/ToggleJoin", ButtonState.JustDown, i, HandleToggleJoin);
      InputRouter.Instance.TryListenButton("MatchSetup/ToggleTeam", ButtonState.JustDown, i, HandleToggleTeam);
      InputRouter.Instance.TryListenButton("MatchSetup/ToggleReady", ButtonState.JustDown, i, HandleToggleReady);
      InputRouter.Instance.TryListenButton("MatchSetup/ChangeName", ButtonState.JustDown, i, HandleChangeName);
    }
  }

  void OnDestroy() {
    InputRouter.Instance.DeviceConnected.Unlisten(AddDevice);
    InputRouter.Instance.DeviceDisconnected.Unlisten(RemoveDevice);
    for (var i = 0; i < InputRouter.MAX_PORT_COUNT; i++) {
      InputRouter.Instance.TryUnlistenButton("MatchSetup/ToggleJoin", ButtonState.JustDown, i, HandleToggleJoin);
      InputRouter.Instance.TryUnlistenButton("MatchSetup/ToggleTeam", ButtonState.JustDown, i, HandleToggleTeam);
      InputRouter.Instance.TryUnlistenButton("MatchSetup/ToggleReady", ButtonState.JustDown, i, HandleToggleReady);
      InputRouter.Instance.TryUnlistenButton("MatchSetup/ChangeName", ButtonState.JustDown, i, HandleChangeName);
    }
  }

  void HandleToggleJoin(PortButtonState action) {
    ToggleJoin(action.PortIndex);
  }

  void HandleToggleTeam(PortButtonState action) {
    ToggleTeam(action.PortIndex);
  }

  void HandleChangeName(PortButtonState action) {
    ChangeName(action.PortIndex);
  }

  void HandleToggleReady(PortButtonState action) {
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
      MatchManager.Instance.StartMatch(Players.Where(p => p.State == PotentialPlayerState.Ready), MatchConfig);
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