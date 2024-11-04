using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(ScriptExecutionGroups.Late)]
public class AbilityManager : MonoBehaviour {
  [HideInInspector, NonSerialized] List<Ability> Abilities = new();

  public IEnumerable<Ability> Running => Abilities.Where(a => a.IsRunning);
  IEnumerable<Ability> Cancellable => Abilities.Where(a => a.IsRunning && a.ActiveTags.HasAllFlags(AbilityTag.Cancellable | AbilityTag.OnlyOne));
  public void CancelAbilities() => Cancellable.ForEach(a => a.Stop());

  public void AddAbility(Ability ability) => Abilities.Add(ability);
  public void RemoveAbility(Ability ability) {
    ability.Stop();
    Abilities.Remove(ability);
  }

  public bool CanRun(Ability ability) {
    bool CanCancel(Ability other) => ability.StartingTags.HasAllFlags(AbilityTag.CancelOthers) && other.ActiveTags.HasAllFlags(AbilityTag.Cancellable);

    var failReason = 0 switch {
      _ when !ability.CanRun(ability.RunEvent) => 1,
      //_ when !Status.CanAttack => 2,
      _ when ability.StartingTags.HasAllFlags(AbilityTag.OnlyOne) && Running.Any(a => a.ActiveTags.HasAllFlags(AbilityTag.OnlyOne) && !CanCancel(a)) => 4,
      _ when ability.StartingTags.HasAllFlags(AbilityTag.BlockIfRunning) && ability.IsRunning => 5,
      _ when ability.StartingTags.HasAllFlags(AbilityTag.BlockIfNotRunning) && !ability.IsRunning => 6,
      //_ when ability.TagsWhenActive.HasAllFlags(AbilityTag.Grounded) && !Status.IsGrounded => 7,
      //_ when ability.TagsWhenActive.HasAllFlags(AbilityTag.Airborne) && Status.IsGrounded => 8,
      _ => 0,
    };
    //if (failReason > 0)
    //  Debug.Log($"Trying to start {ability}.{method.Method.Name} but cant because {failReason}");
    return failReason == 0;
  }

  public bool TryRun(Ability ability) {
    if (CanRun(ability)) {
      Run(ability);
      return true;
    }
    return false;
  }

  public bool TryRun<T>(Ability ability, T parameter) {
    if (CanRun(ability)) {
      Run(ability, parameter);
      return true;
    }
    return false;
  }

  public void Run(Ability ability) {
    CancelAbilities(); // TODO: only do if needed
    ability.RunEvent.Fire();
  }

  public void Run<T>(Ability ability, T parameter) {
    CancelAbilities();
    ((IAbilityWithParameter<T>)ability).Parameter = parameter;
    ability.RunEvent.Fire();
  }

  public void Stop(Ability ability) {
    if (ability.IsRunning)
      ability.Stop();
  }

  public TaskFunc RunUntilDone(Ability ability) => async s => {
    Run(ability);
    await s.Until(() => !ability.IsRunning);
  };

  void OnDestroy() => Abilities.ForEach(a => a.Stop());
}