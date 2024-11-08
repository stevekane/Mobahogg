using UnityEngine;

public class MoveAbility : Ability, IAbilityWithParameter<Vector2> {
  public Vector2 Parameter { get; set; }

  protected override void Awake() {
    base.Awake();
    RunEvent.Listen(() => Tick());
  }
  protected override void OnDestroy() {
    base.OnDestroy();
    RunEvent.Clear();
  }

  void Tick() {
    var dir = Parameter.XZ();
    OwnerComponent<Mover>().SetDesiredMoveAndFacing(dir, dir.TryGetDirection() ?? AbilityManager.transform.forward);
  }
}