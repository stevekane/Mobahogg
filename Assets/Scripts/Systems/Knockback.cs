using UnityEngine;

// TODO: COuld remove direct reference to CharacterController and use Event
[DefaultExecutionOrder((int)ExecutionGroups.State)]
public class Knockback : MonoBehaviour {
  static Vector3 Longer(Vector3 x, Vector3 y) =>
    Vector3.SqrMagnitude(x) > Vector3.SqrMagnitude(y)
      ? x
      : y;

  const float MIN_MAGNITUDE = 0.01f;

  [SerializeField] LocalClock LocalClock;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] float DecayRate = 2.0f;

  Vector3 Current;
  Vector3? Next = null;

  public void Set(Vector3 knockback) {
    if (knockback.sqrMagnitude == 0)
      return;
    Next = Next.HasValue ? Longer(knockback, Next.Value) : knockback;
  }

  void FixedUpdate() {
    float dt = LocalClock.DeltaTime();
    if (Next.HasValue) {
      Current = Next.Value;
    } else {
      Current *= Mathf.Pow(Mathf.Clamp01(1 - dt * DecayRate), DecayRate);
      Current = Current.magnitude <= MIN_MAGNITUDE ? Vector3.zero : Current;
    }
    Next = null;
    CharacterController.DirectVelocity.Add(Current);
  }
}