// Forwards its RunEvent activation to another Ability, to allow composite actions to be handled by one class (the one we forward to).
public class SubAbility : Ability {
  public AbilityEventReference Event;

  Ability Ability;
  IEventSource AbilityEvent;

  public override AbilityTag ActiveTags => Tags | Ability.ActiveTags;

  public override bool CanRun(IEventSource _) => Ability.CanRun(AbilityEvent);

  protected override void Awake() {
    base.Awake();
    Ability = Event.Ability;
    AbilityEvent = Event.GetEvent();
    RunEvent.Listen(() => AbilityEvent.Fire());
  }
  protected override void OnDestroy() {
    base.OnDestroy();
    RunEvent.Clear();
  }
}