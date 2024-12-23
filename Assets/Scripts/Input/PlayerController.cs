using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  public int PortIndex;

  void Start() {
    InputRouter.Instance?.TryListenValue("Move", PortIndex, HandleMove);
    InputRouter.Instance?.TryListenButton("Jump", ButtonState.JustDown ,PortIndex, HandleJump);
    InputRouter.Instance?.TryListenButton("Attack", ButtonState.JustDown, PortIndex, HandleAttack);
    InputRouter.Instance?.TryListenButton("Cast Spell", ButtonState.JustDown, PortIndex, HandleCastSpell);
  }

  void OnDestroy() {
    InputRouter.Instance?.TryUnlistenValue("Move", PortIndex, HandleMove);
    InputRouter.Instance?.TryUnlistenButton("Jump", ButtonState.JustDown ,PortIndex, HandleJump);
    InputRouter.Instance?.TryUnlistenButton("Attack", ButtonState.JustDown, PortIndex, HandleAttack);
    InputRouter.Instance?.TryUnlistenButton("Cast Spell", ButtonState.JustDown, PortIndex, HandleCastSpell);
  }

  // TODO:
  // This is sort of a hack for now to send MoveEvents for any player controller
  // That does not have an active connected device
  // Think of this like a poor-man's AI
  // This has to do with currently needing to invoke "move" whenever possible to sort of
  // exert this character's desired motion. This def does not really feel correct though
  // so maybe there ought to be a seperate system that simply establishes that the next
  // desired velocity is always defaulting to 0 and that you must try to have another velocity
  // explicitly through an effort to move
  void FixedUpdate() {
    if (InputRouter.Instance.HasConnectedDevice(PortIndex))
      return;
    PlayersOnPort.ForEach(p => p.MoveAbility.TryRun(Vector2.zero));
  }

  IEnumerable<Player> PlayersOnPort =>
    LivesManager.Active.Players.Where(p => p.PortIndex == PortIndex);

  void HandleMove(PortValue action) {
    var moveRan = PlayersOnPort.Any(p => p.MoveAbility.TryRun(action.Value));
    var turnRan = PlayersOnPort.Any(p => p.TurnAbility.TryRun(action.Value));
  }

  public void HandleJump(PortButtonState action) {
    var anyRan = PlayersOnPort.Any(p => p.TryJump());
    if (anyRan) {
      InputRouter.Instance.ConsumeButton("Jump", PortIndex);
    }
  }

  public void HandleAttack(PortButtonState action) {
    InputRouter.Instance.TryGetValue("Move", PortIndex, out var direction);
    var anyRan = PlayersOnPort.Any(p => p.AttackAbility.TryRun(direction));
    if (anyRan) {
      InputRouter.Instance.ConsumeButton("Attack", PortIndex);
    }
  }

  public void HandleCastSpell(PortButtonState action) {
    var anyRan = PlayersOnPort.Any(p => p.SpellCastAbility.TryRun());
    if (anyRan) {
      InputRouter.Instance.ConsumeButton("Cast Spell", PortIndex);
    }
  }
}