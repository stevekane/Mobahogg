public interface IAbility {
  public bool CanRun { get; }
  public bool TryRun();
}

public interface IAbility<T> {
  public bool CanRun { get; }
  public bool TryRun(T t);
}