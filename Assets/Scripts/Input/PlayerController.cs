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
    var jumpJustDown = InputRouter.Instance.JustDownWithin("Jump", PortIndex, frameBufferDuration);
    var attackJustDown = InputRouter.Instance.JustDownWithin("Attack", PortIndex, frameBufferDuration);
    var dashJustDown = InputRouter.Instance.JustDownWithin("Dash", PortIndex, frameBufferDuration);
    var activeSpellJustDown = InputRouter.Instance.JustDownWithin("Active", PortIndex, frameBufferDuration);
    var ultimateSpellJustDown = InputRouter.Instance.JustDownWithin("Ultimate", PortIndex, frameBufferDuration);
    InputRouter.Instance.TryGetValue("Move", PortIndex, out var move);
    InputRouter.Instance.TryGetButtonState("Jump", PortIndex, out var jumpButtonState);
    InputRouter.Instance.TryGetButtonState("Active", PortIndex, out var activeButtonState);

    /*
    A somewhat common use case is that an ability can be sort of .... started and then ultimately
    released.

    This use-case really doesn't have that much fundamentally to do with buttons but more so with
    the actions taken by the thing executing the ability.

    You may, however, want to know when an ability can be released and then somehow trigger that release
    by inspecting the current state of the input that you've chosen to associate with that ability.

    In our case, we simply want to know whether an ability can be released and if its button is actually
    down on the current frame.

    It could be argued that you actually care about whether there was a "just released" in the past
    but it's probably enough to just know the button state NOW. therefore, we will add this universal notion
    of releasability to our Active and Ultimate abilities and consider how we might do this more generally.
    */

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