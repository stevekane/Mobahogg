using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using Cysharp.Threading.Tasks.Triggers;

public abstract class AbstractAsyncAbility : MonoBehaviour {
  [SerializeField] UnityEvent OnStart;

  UniTaskCompletionSource TaskCompletionSource;
  CancellationTokenSource TokenSource;

  public abstract bool CanRun();
  public abstract UniTask Run(CancellationToken token);
  public bool IsRunning => TokenSource != null;
  public bool TryRun() {
    if (CanRun()) {
      OnStart.Invoke();
      RunTask().Forget();
      return true;
    } else {
      return false;
    }
  }

  async UniTask RunTask() {
    try {
      if (TaskCompletionSource != null && !TaskCompletionSource.Task.Status.IsCompleted()) {
        TokenSource?.Cancel();
        await TaskCompletionSource.Task;
      }
      TokenSource?.Dispose();
      TokenSource = null;
      TokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);
      TaskCompletionSource = new();
      await Run(TokenSource.Token);
    } finally {
      TaskCompletionSource.TrySetResult();
      TokenSource?.Dispose();
      TokenSource = null;
    }
  }

  public void Cancel() {
    // Debug.Log("Cancel Called");
    TokenSource?.Cancel();
  }
  void Cleanup() {
    // Debug.Log("Cleanup Called");
    TokenSource?.Dispose();
    TokenSource = null;
  }
}