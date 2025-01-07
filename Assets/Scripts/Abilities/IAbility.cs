namespace Abilities {
  public interface IAbility {
    public bool CanRun { get; }
    public void Run();
  }

  public interface IAbility<T> {
    public bool CanRun { get; }
    public void Run(T t);
  }

  public interface Async {
    public bool IsRunning { get; }
  }

  public interface Cancellable {
    public bool CanCancel { get; }
    public void Cancel();
  }
}