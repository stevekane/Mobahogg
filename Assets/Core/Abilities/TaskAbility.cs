using System.Threading.Tasks;

// Convenience machinery which binds an Ability's RunEvent to an async Run method. Is considered
// "Running" as long as its TaskRunner has any running tasks.
public abstract class TaskAbility : Ability {
  public override bool IsRunning => RunningTaskCount > 0;
  public override void Stop() {
    Tags = default;
    TaskRunner.StopAllTasks();
  }
  // Main entry-point that runs while ability is running.
  public abstract Task Run(TaskScope scope);

  // TODO: Perhaps this could be simplified. We could just be "Running" until Run/Runner returns. I'm not sure
  // there will be any alternate consumers of Runner/Run.
  int RunningTaskCount = 0;
  protected TaskRunner TaskRunner = new();

  protected void OnEvent(TaskFunc f) {
    TaskRunner.RunTask(Runner(f));
  }
  protected TaskFunc Runner(TaskFunc f) => async scope => {
    try {
      RunningTaskCount++;
      Tags.AddFlags(StartingTags);
      if (f(scope) is var task && task != null)
        await task;
    } finally {
      RunningTaskCount--;
      if (RunningTaskCount == 0)
        Stop();  // TODO: This is technically wrong. In the cancellation case, Stop will be called twice.
    }
  };

  protected override void Awake() {
    base.Awake();
    RunEvent.Listen(() => OnEvent(Run));
  }
  protected override void OnDestroy() {
    base.OnDestroy();
    TaskRunner?.Dispose();
    RunEvent.Clear();
  }
  protected virtual void FixedUpdate() {
    TaskRunner.FixedUpdate();
  }
}