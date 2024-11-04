using System;
using System.Reflection;

// An editor reference to an EventSource on an Ability. Used by SubAbility to point to events other than Ability.RunEvent.
[Serializable]
public class AbilityEventReference {
  public Ability Ability;
  public string EventName;

  public IEventSource GetEvent() {
    if (!Ability || EventName.Length == 0) return null;
    var fieldInfo = Ability.GetType().GetField(EventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    return (IEventSource)fieldInfo.GetValue(Ability);
  }
}
