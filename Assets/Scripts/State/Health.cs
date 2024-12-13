using UnityEngine;

namespace State {
  public class Health : AbstractState {
    [SerializeField] int Current = 1;
    [SerializeField] int Max = 1;
    int CurrentAccumulator;
    int MaxAccumulator;

    public EventSource OnChange = new();

    public void Change(int delta) => CurrentAccumulator += delta;
    public void ChangeMax(int delta) => MaxAccumulator += delta;
    public int CurrentValue => Current;
    public int MaxValue => Max;

    void FixedUpdate() {
      Max += MaxAccumulator;
      Max = Mathf.Max(0, Max);
      Current += CurrentAccumulator;
      Current = Mathf.Min(Max, Mathf.Max(0, Current));
      if (CurrentAccumulator != 0 || MaxAccumulator != 0) {
        OnChange.Fire();
        CurrentAccumulator = 0;
        MaxAccumulator = 0;
      }
    }
  }
}