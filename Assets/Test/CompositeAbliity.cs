using System.Threading.Tasks;
using UnityEngine;

// Test ability with multiple entry/exit points. SubAbility helps bind to these events.
public class CompositeAbility : TaskAbility {
  public EventSource ReleaseEvent = new();
  public EventSource AltEvent = new();

  public override bool CanRun(IEventSource entry) => 0 switch {
    _ when entry == RunEvent => !IsRunning,
    _ when entry == ReleaseEvent => IsRunning,
    _ when entry == AltEvent => IsRunning,
    _ => false,
  };

  public override async Task Run(TaskScope scope) {
    var anim = AbilityManager.GetComponent<Animator>();
    anim.SetTrigger("SwingSword");
    await scope.Any(
      Waiter.ListenFor(ReleaseEvent),
      Waiter.Repeat(async s => {
        await s.ListenFor(AltEvent);
        Debug.Log($"Alt {name}");
      }));
  }
}