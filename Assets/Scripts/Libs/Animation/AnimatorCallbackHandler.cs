using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class AnimatorCallbackHandler : MonoBehaviour {
  public Animator Animator;
  public readonly EventSource OnRootMotion = new();
  public readonly EventSource<int> OnIK = new();
  public readonly EventSource<string> OnEvent = new();

  void OnAnimatorIK(int layer) => OnIK.Fire(layer);

  void OnAnimatorMove() => OnRootMotion.Fire();
}