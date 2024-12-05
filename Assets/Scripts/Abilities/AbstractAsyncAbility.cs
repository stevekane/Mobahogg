using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public abstract class AbstractAsyncAbility : MonoBehaviour {
  protected CancellationTokenSource TokenSource;

  public abstract bool CanRun();
  public abstract UniTask Run(CancellationToken token);
  public bool IsRunning => TokenSource != null;
  public bool TryRun() {
    if (CanRun()) {
      TokenSource?.Cancel();
      TokenSource?.Dispose();
      TokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);
      Run(TokenSource.Token).ContinueWith(Cleanup).Forget();
      return true;
    } else {
      return false;
    }
  }
  void Cleanup() {
    TokenSource?.Cancel();
    TokenSource?.Dispose();
    TokenSource = null;
  }
}