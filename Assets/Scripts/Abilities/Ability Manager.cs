using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abilities {
  public class AbilityManager : MonoBehaviour {
    public List<RegisteredAbility> RegisteredAbilities;
    public LocalClock LocalClock;
    public Animator Animator;
    public AnimatorCallbackHandler AnimatorCallbackHandler;
    public KCharacterController CharacterController;

    public EventSource<Ability> OnRegisterAbility = new();
    public EventSource<Ability> OnUnregisterAbility = new();

    void Start() {
      GetComponentsInChildren<Ability>().ForEach(Register);
    }

    public void Register(Ability ability) {
      var registeredAbility = RegisteredAbility.From(ability);
      registeredAbility.Ability.Register(this);
      RegisteredAbilities.Add(registeredAbility);
      OnRegisterAbility.Fire(ability);
    }

    public void Unregister(Ability ability) {
      var registeredAbility = RegisteredAbilities.First(ra => ra.Ability == ability);
      RegisteredAbilities.Remove(registeredAbility);
      OnUnregisterAbility.Fire(ability);
    }

    public bool CanRun(Ability ability) {
      var matchingAbility = RegisteredAbilities.First(ra => ra.Ability == ability);
      var shouldRun = true;
      foreach (var registeredAbility in RegisteredAbilities) {
        if (registeredAbility.Ability.IsRunning) {
          var doesBlock = TagSet.Overlap(registeredAbility.AbilityTags.BlockAbilitiesWith, matchingAbility.AbilityTags.Tags);
          var shouldCancel = TagSet.Overlap(registeredAbility.AbilityTags.Tags, matchingAbility.AbilityTags.CancelAbilitiesWith);
          var canCancel = registeredAbility.Ability.CanStop;
          shouldRun = !doesBlock || (shouldCancel && canCancel);
        }
      }
      return shouldRun && matchingAbility.Ability.CanRun;
    }

    public bool TryRun(Ability ability) {
      if (CanRun(ability)) {
        Run(ability);
        return true;
      } else {
        return false;
      }
    }

    public void Run(Ability ability) {
      var matchingAbility = RegisteredAbilities.FirstOrDefault(ra => ra.Ability == ability);
      if (matchingAbility == null)
        return;

      foreach (var registeredAbility in RegisteredAbilities) {
        var shouldCancel = TagSet.Overlap(registeredAbility.AbilityTags.Tags, matchingAbility.AbilityTags.CancelAbilitiesWith);
        var canCancel = registeredAbility.Ability.CanStop;
        if (shouldCancel && canCancel) {
          registeredAbility.Ability.Stop();
        }
      }
      ability.Run();
    }
  }

  [Serializable]
  public class RegisteredAbility {
    public Ability Ability;
    public AbilityTags AbilityTags;
    public static RegisteredAbility From(Ability ability) {
      return new RegisteredAbility {
        Ability = ability,
        AbilityTags = ability.GetComponentInChildren<AbilityTags>()
      };
    }
  }

}