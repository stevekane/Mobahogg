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
    InputRouter.Instance?.TryListenButton("Dash", ButtonState.JustDown, PortIndex, HandleDash);
  }

  void OnDestroy() {
    InputRouter.Instance?.TryUnlistenValue("Move", PortIndex, HandleMove);
    InputRouter.Instance?.TryUnlistenButton("Jump", ButtonState.JustDown ,PortIndex, HandleJump);
    InputRouter.Instance?.TryUnlistenButton("Attack", ButtonState.JustDown, PortIndex, HandleAttack);
    InputRouter.Instance?.TryUnlistenButton("Dash", ButtonState.JustDown, PortIndex, HandleDash);
  }

  IEnumerable<Player> PlayersOnPort =>
    LivesManager.Active.Players.Where(p => p.PortIndex == PortIndex);

  void HandleMove(PortValue action) {
    PlayersOnPort.ForEach(p => p.MoveAbility.TryRun(action.Value));
    PlayersOnPort.ForEach(p => p.TurnAbility.TryRun(action.Value));
  }

  public void HandleJump(PortButtonState action) {
    var anyRan = PlayersOnPort.Any(p => p.JumpAbility.TryRun());
    if (anyRan) {
      InputRouter.Instance.ConsumeButton("Jump", PortIndex);
    }
  }

  public void HandleAttack(PortButtonState action) {
    InputRouter.Instance.TryGetValue("Move", PortIndex, out var direction);
    var anyRan = PlayersOnPort.Any(p => {
      return p.Grounded
        ? p.AttackAbility.TryRun(direction)
        : p.AirAttackAbility.TryRun(direction);
    });

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

  public void HandleDash(PortButtonState action) {
    InputRouter.Instance.TryGetValue("Move", PortIndex, out var direction);
    var anyRan = PlayersOnPort.Any(p => p.DiveRollAbility.TryRun(direction));
    if (anyRan) {
      InputRouter.Instance.ConsumeButton("Dash", PortIndex);
    }
  }
}