using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : SingletonBehavior<PlayerManager> {
  public GameObject PlayerPrefab;
  public Transform SpawnTeam1;
  public Transform SpawnTeam2;
  Inputs Inputs;
  Dictionary<Player, InputDevice> PlayerGamepads = new();

  bool JoinWasPressed = false;
  protected override void AwakeSingleton() {
    Inputs = new();
    Inputs.Player.JoinTeam1.performed += ctx => PlayerPressedJoin(ctx, 1);
    Inputs.Player.JoinTeam2.performed += ctx => PlayerPressedJoin(ctx, 2);
  }
  void OnEnable() => Inputs.Enable();
  void OnDisable() => Inputs.Disable();

  public void RegisterPlayer(Player player) {
    if (!JoinWasPressed) {
      // Special case for debugging - a Player exists in the scene. Assign them a gamepad.
      var gamepad = Gamepad.all.FirstOrDefault(g => g.name.Contains("DualShock")) ?? Gamepad.current;
      InitPlayer(player, gamepad);
    }
    // Otherwise, PlayerPressedStart will call InitPlayer with the proper device.
  }
  public void UnregisterPlayer(Player player) {
    PlayerGamepads.Remove(player);
  }

  void PlayerPressedJoin(InputAction.CallbackContext ctx, int teamID) {
    JoinWasPressed = true;
    var kv = PlayerGamepads.FirstOrDefault(kv => kv.Value == ctx.control.device);
    if (kv.Key == null) {
      var player = SpawnPlayer(teamID);
      InitPlayer(player, ctx.control.device);
    } else {
      kv.Key.gameObject.Destroy();
    }
  }

  Player SpawnPlayer(int teamID) {
    var spawn = teamID == 1 ? SpawnTeam1 : SpawnTeam2;
    if (PlayerPrefab)
      return Instantiate(PlayerPrefab, spawn.position, spawn.rotation).GetComponent<Player>();
    return null;
  }

  void InitPlayer(Player player, InputDevice device) {
    if (device != null) {
      // Special case for debugging - existing player gets mouse/keyboard.
      player.GetComponent<InputMappings>().AssignDevices(
        !JoinWasPressed ? new InputDevice[] { device, Keyboard.current, Mouse.current } : new InputDevice[] { device });
    }
    PlayerGamepads[player] = device;
  }
}