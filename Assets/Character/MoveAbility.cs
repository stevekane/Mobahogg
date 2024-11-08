using UnityEngine;

public class MoveAbility : Ability, IAbilityWithParameter<Vector2> {
  public Vector2 Parameter { get; set; }
  public float MoveSpeed = 5f;

  protected override void Awake() {
    base.Awake();
    RunEvent.Listen(() => Tick());
  }
  protected override void OnDestroy() {
    base.OnDestroy();
    RunEvent.Clear();
  }

  void Tick() {
    if (Parameter.sqrMagnitude > 0) {
      var delta = Time.fixedDeltaTime * MoveSpeed * Parameter.XZ();
      OwnerComponent<Mover>().Move(delta);
      Camera.main.transform.position += delta;
    }
  }
}