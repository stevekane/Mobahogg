using UnityEngine;

namespace State {
  public class MaxHealth : AbstractState {
    int Current;
    int Next;
    int Value => Current;
    void Init(int v) => Current = Next = v;
    void Set(int next) => Next = Mathf.Max(next, Next);
    void FixedUpdate() {
      Current = Next;
    }
  }
}