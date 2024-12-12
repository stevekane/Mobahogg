using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class Tasks {
  public static async UniTask Delay(
  int frames,
  LocalClock localClock,
  CancellationToken token) {
    while (frames > 0) {
      await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate, token);
      frames -= localClock.DeltaFrames();
    }
  }

  public static async UniTask EveryFrame(
  int frames,
  LocalClock localClock,
  Action<int> action,
  CancellationToken token) {
    var frame = 0;
    while (frame <= frames) {
      token.ThrowIfCancellationRequested();
      action(frame);
      frame += localClock.DeltaFrames();
      await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
    }
  }

  public static async UniTask Tween(
  float start,
  float end,
  LocalClock localClock,
  int frames,
  Func<float,float> easingFunction,
  Action<float> action,
  CancellationToken token) {
    await EveryFrame(frames, localClock, frame => action(Mathf.Lerp(start, end, easingFunction((float)frame/frames))), token);
  }

  // This runs in LOCAL space of the moving object. Maybe make this flexible / optional?
  public static async UniTask MoveBy(
  Transform t,
  Vector3 destination,
  LocalClock clock,
  int frames,
  Func<float, float> xEasing,
  Func<float, float> yEasing,
  Func<float, float> zEasing,
  CancellationToken token) {
    var localToWorldMatrix = t.localToWorldMatrix;
    var start = Vector3.zero;
    var end = destination;
    await EveryFrame(frames, clock, frame => {
      var x = Mathf.Lerp(start.x, end.x, xEasing((float)frame/frames));
      var y = Mathf.Lerp(start.y, end.y, yEasing((float)frame/frames));
      var z = Mathf.Lerp(start.z, end.z, zEasing((float)frame/frames));
      t.position = localToWorldMatrix.MultiplyPoint3x4(new(x,y,z));
    }, token);
  }


  public static async UniTask ListenFor(EventSource source, CancellationToken token) {
    var fired = false;
    void CB() => fired = true;
    bool WAS_FIRED() => fired;
    try {
      source.Listen(CB);
      await UniTask.WaitUntil(WAS_FIRED, cancellationToken: token);
    } finally {
      source.Unlisten(CB);
    }
  }

  public static async UniTask<T> ListenFor<T>(EventSource<T> source, CancellationToken token) {
    var completionSource = new UniTaskCompletionSource<T>();
    var fired = false;
    void CB(T t) => fired = true;
    void SET(T t) => completionSource.TrySetResult(t);
    bool WAS_FIRED() => fired;
    try {
      source.Listen(CB);
      source.Listen(SET);
      await UniTask.WaitUntil(WAS_FIRED, cancellationToken: token);
    } finally {
      source.Unlisten(CB);
      source.Unlisten(SET);
    }
    return await completionSource.Task;
  }

  public static async UniTask Forever(Func<CancellationToken, UniTask> task, CancellationToken token) {
    while (true) await task(token);
  }

  public static async UniTask First(UniTask l, UniTask r) {
    var lSrc = new CancellationTokenSource();
    var rSrc = new CancellationTokenSource();
    try {
      var index = await UniTask.WhenAny(
        l.AttachExternalCancellation(lSrc.Token),
        r.AttachExternalCancellation(rSrc.Token));
      if (index == 0)
        rSrc.Cancel();
      else
        lSrc.Cancel();
    } finally {
        lSrc.Dispose();
        rSrc.Dispose();
    }
  }

  public static async UniTask DoEveryFrameForDuration(
  float duration,
  Action<float, float> action,
  CancellationToken token) {
    float elapsed = 0f;
    while (elapsed < duration) {
      elapsed += Time.deltaTime;
      token.ThrowIfCancellationRequested();
      action(Time.deltaTime, elapsed);
      await UniTask.Yield(cancellationToken: token);
    }
  }
}