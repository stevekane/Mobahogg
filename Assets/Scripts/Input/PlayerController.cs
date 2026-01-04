using System.Linq;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Input+1)]
public class PlayerController : MonoBehaviour {
  public int PortIndex;

  public Player Player => SpawnManager.Active.Players.FirstOrDefault(p => p.PortIndex == PortIndex);

  void FixedUpdate() {
    var player = Player;
    if (!player)
      return;
    var frameBufferDuration = InputRouter.Instance.Settings.InputBufferFrameWindow;
    var jumpJustDown = InputRouter.Instance.JustDownWithin("Jump", PortIndex, frameBufferDuration);
    var attackJustDown = InputRouter.Instance.JustDownWithin("Attack", PortIndex, frameBufferDuration);
    var dashJustDown = InputRouter.Instance.JustDownWithin("Dash", PortIndex, frameBufferDuration);
    var activeSpellJustDown = InputRouter.Instance.JustDownWithin("Active", PortIndex, frameBufferDuration);
    var ultimateSpellJustDown = InputRouter.Instance.JustDownWithin("Ultimate", PortIndex, frameBufferDuration);
    InputRouter.Instance.TryGetValue("Move", PortIndex, out var move);
    InputRouter.Instance.TryGetButtonState("Hover", PortIndex, out var hoverButtonState);
    InputRouter.Instance.TryGetButtonState("Active", PortIndex, out var activeButtonState);

    // Buttons
    // Jump > Dash > Attack > Ultimate > Active > Hover
    if (jumpJustDown && player.CanJump) {
      player.Jump();
      InputRouter.Instance.ConsumeButton("Jump", PortIndex);
    }
    if (dashJustDown && player.CanDash) {
      player.Dash(move);
      InputRouter.Instance.ConsumeButton("Dash", PortIndex);
    }
    if (attackJustDown && player.CanAttack) {
      player.Attack(move);
      InputRouter.Instance.ConsumeButton("Attack", PortIndex);
    }
    if (ultimateSpellJustDown && player.CanUseUltimateAbility) {
      player.UseUltimateAbility(move);
      InputRouter.Instance.ConsumeButton("Ultimate", PortIndex);
    }
    if (activeSpellJustDown && player.CanUseActiveAbility) {
      player.UseActiveAbility(move);
      InputRouter.Instance.ConsumeButton("Active", PortIndex);
    }
    if (activeButtonState == ButtonState.JustUp || activeButtonState == ButtonState.Up) {
      player.ReleaseActiveAbility();
    }
    if (player.CanHover && hoverButtonState == ButtonState.Down) {
      player.StartHover();
    } else if (player.CanEndHover) {
      player.EndHover();
    }

    // Sticks
    // Move > Steer
    if (player.CanMove) {
      player.Move(move);
    }
    if (player.CanSteer) {
      player.Steer(move);
    }
  }
}