using System.Linq;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Input+1)]
public class PlayerController : MonoBehaviour {
  public int PortIndex;

  public Player Player => LivesManager.Active.Players.FirstOrDefault(p => p.PortIndex == PortIndex);

  void FixedUpdate() {
    var player = Player;
    if (!player)
      return;
    InputRouter.Instance.TryGetValue("Move", PortIndex, out var move);
    InputRouter.Instance.TryGetButtonState("Jump", PortIndex, out var jump);
    InputRouter.Instance.TryGetButtonState("Attack", PortIndex, out var attack);
    InputRouter.Instance.TryGetButtonState("Dash", PortIndex, out var dash);
    InputRouter.Instance.TryGetButtonState("Cast Spell", PortIndex, out var spell);

    // Buttons
    // Jump > Dash > Attack > Spell > Hover
    if (player.CanJump && jump == ButtonState.JustDown) {
      player.Jump();
      InputRouter.Instance.ConsumeButton("Jump", PortIndex);
    }
    if (player.CanDash && dash == ButtonState.JustDown) {
      player.Dash(move);
      InputRouter.Instance.ConsumeButton("Dash", PortIndex);
    }
    if (player.CanAttack && attack == ButtonState.JustDown) {
      player.Attack(move);
      InputRouter.Instance.ConsumeButton("Attack", PortIndex);
    }
    if (player.CanCastSpell && spell == ButtonState.JustDown) {
      player.CastSpell(move);
      InputRouter.Instance.ConsumeButton("Cast Spell", PortIndex);
    }
    if (player.CanHover && jump == ButtonState.Down) {
      player.Hover();
    } else {
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