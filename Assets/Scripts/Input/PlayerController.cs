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
    var frameBufferDuration = SettingsManager.Instance.InputSettings.InputBufferFrameWindow;
    InputRouter.Instance.TryGetValue("Move", PortIndex, out var move);
    InputRouter.Instance.TryGetButtonState("Jump", PortIndex, out var jumpButtonState);
    var jumpJustDown = InputRouter.Instance.JustDownWithin("Jump", PortIndex, frameBufferDuration);
    var attackJustDown = InputRouter.Instance.JustDownWithin("Attack", PortIndex, frameBufferDuration);
    var dashJustDown = InputRouter.Instance.JustDownWithin("Dash", PortIndex, frameBufferDuration);
    var castSpellJustDown = InputRouter.Instance.JustDownWithin("Cast Spell", PortIndex, frameBufferDuration);

    // Buttons
    // Jump > Dash > Attack > Spell > Hover
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
    if (castSpellJustDown && player.CanCastSpell) {
      player.CastSpell(move);
      InputRouter.Instance.ConsumeButton("Cast Spell", PortIndex);
    }
    if (player.CanHover && jumpButtonState == ButtonState.Down) {
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