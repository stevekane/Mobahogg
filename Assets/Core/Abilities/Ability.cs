using UnityEngine;

  // Optional parameter for abilities, ex: a movement ability with a Vector2 axis value.
public interface IAbilityWithParameter<T> {
  public T Parameter { get; set; }
}

// Base class for Abilities. Managed and run via AbilityManager, bound to inputs via InputMappings.
// The Ability is activated via its RunEvent, which should be bound to a callback.
// Subclasses exist with extra machinery for common cases.
// - TaskAbility: binds RunEvent to the async `Run` method.
// - SubAbility: forwards RunEvent to another Ability to allow multiple events/bindings to be handled by a single composite Ability.
//
// TODO: Tag system TBD. Want a balance of ease-of-use and flexibility. Some tags should be read-only functions of character state,
// e.g. "CanMove" or "CanAct". Some tags should be added/removed by active abilities and restored when the ability ends? Maybe?
[DefaultExecutionOrder(ScriptExecutionGroups.Ability)]
public abstract class Ability : MonoBehaviour {
  // Main entry point. Use SubAbility to bind an ability to an event source other than Main.
  public EventSource RunEvent = new();
  public AbilityTag StartingTags;

  public virtual AbilityTag ActiveTags => Tags;
  public virtual bool IsRunning => false;
  public virtual bool CanRun(IEventSource entry) => true;
  public virtual void Stop() { Tags = default;  }

  protected AbilityManager AbilityManager;
  protected AbilityTag Tags;

  protected virtual void Awake() {
    AbilityManager = GetComponentInParent<AbilityManager>();
    if (AbilityManager)  // Ability may be detached, e.g. part of an item
      AbilityManager.AddAbility(this);
  }

  protected virtual void OnDestroy() {
    if (AbilityManager)
      AbilityManager.RemoveAbility(this);
  }
}