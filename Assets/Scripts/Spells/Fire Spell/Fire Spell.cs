using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireSpell : Spell {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] GameObject FireballPrefab;
  // TODO: Move to FireSpellSettings
  [SerializeField] float SpreadAngle = 60;
  [SerializeField] float FireballSpeed = 20;
  [SerializeField] int Count = 5;
  [SerializeField] int FanoutFrames = 5;
  [SerializeField] int TravelFrames = 60 * 5;

  List<GameObject> Fireballs = new(5);
  List<Quaternion> Orientations = new(5);

  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    Run(position, rotation, this.destroyCancellationToken).Forget();
  }

  async UniTask Run(Vector3 position, Quaternion rotation, CancellationToken token) {
    var forward = rotation * Vector3.forward;
    var left = Quaternion.LookRotation(Quaternion.Euler(0, -SpreadAngle / 2, 0) * forward);
    var right = Quaternion.LookRotation(Quaternion.Euler(0, SpreadAngle / 2, 0) * forward);
    for (var i = 0; i < Count; i++) {
      var orientation = Quaternion.Slerp(left, right, (float)i/(Count-1));
      var fireball = Instantiate(FireballPrefab, position, orientation, transform);
      Fireballs.Add(fireball);
      Orientations.Add(orientation);
    }
    for (var f = 0; f < FanoutFrames; f += LocalClock.DeltaFrames()) {
      for (var i = 0; i < Count; i++) {
        var fireball = Fireballs[i];
        var orientation = Orientations[i];
        var interpolant = (float)f/(FanoutFrames-1);
        var easedInterpolant = EasingFunctions.EaseInQuint(interpolant);
        var nextRotation = Quaternion.Slerp(orientation, rotation, easedInterpolant);
        var nextPosition = fireball.transform.position + LocalClock.DeltaTime() * FireballSpeed * (nextRotation * Vector3.forward);
        fireball.transform.SetPositionAndRotation(nextPosition, nextRotation);
      }
      await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
    }
    for (var f = 0; f < TravelFrames; f += LocalClock.DeltaFrames()) {
      for (var i = 0; i < Count; i++) {
        var fireball = Fireballs[i];
        fireball.transform.position = fireball.transform.position + LocalClock.DeltaTime() * 1.5f *FireballSpeed * fireball.transform.forward;
      }
      await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
    }
    Destroy(gameObject);
  }
}