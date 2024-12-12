using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

public class AirSpell : Spell {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] GameObject AirBallPrefab;
  [SerializeField] GameObject TornadoPrefab;
  [SerializeField] Vector3 Delta = new(0, 10, 10);
  [SerializeField] int TravelFrames = 60;
  [SerializeField] int HoverFrames = 60 * 10;
  [SerializeField] int MinSpinSpeed = 360;
  [SerializeField] int MaxSpinSpeed = 360 * 8;

  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    Run(position, rotation, this.destroyCancellationToken).Forget();
  }

  async UniTask Run(Vector3 start, Quaternion rotation, CancellationToken token) {
    var airball = Instantiate(AirBallPrefab, start, rotation, transform);
    var launch = Tasks.MoveBy(
      airball.transform,
      Delta,
      LocalClock,
      TravelFrames,
      EasingFunctions.Linear,
      EasingFunctions.EaseOutQuint,
      EasingFunctions.EaseInQuint,
      token);
    var spinUp = Tasks.Tween(
      MinSpinSpeed,
      MaxSpinSpeed,
      LocalClock,
      TravelFrames,
      EasingFunctions.EaseInQuint,
      f => airball.GetComponentInChildren<LocalClockSpinner>().DegreesPerSecond = f,
      token);
    await UniTask.WhenAll(launch, spinUp);
    Instantiate(TornadoPrefab, airball.transform.position, Quaternion.identity, transform);
    await Tasks.Delay(HoverFrames, LocalClock, token);
    Destroy(gameObject);
  }
}