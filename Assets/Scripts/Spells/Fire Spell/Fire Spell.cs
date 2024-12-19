using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireSpell : Spell {
  [SerializeField] FireSpellSettings Settings;
  [SerializeField] LocalClock LocalClock;

  // TODO: Could get rid of this list here and pre-allocate on some global physics manager
  Collider[] Colliders = new Collider[32];

  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    Run(position, rotation, owner, this.destroyCancellationToken).Forget();
  }

  async UniTask Run(Vector3 position, Quaternion rotation, Player owner, CancellationToken token) {
    var eggGO = Instantiate(Settings.EggPrefab, position, rotation, transform);
    var egg = eggGO.GetComponent<FireSpellEgg>();
    egg.Owner = owner.gameObject;
    var eggCollision = Tasks.ListenFor(egg.OnCollision, token);
    var travelEasingFn = EasingFunctions.FromName(Settings.EggTravelEasingFunctionName);
    var travel = Tasks.MoveBy(
      egg.transform,
      Settings.EggLocalTravelDelta,
      LocalClock,
      Settings.EggTravelDuration.Ticks,
      travelEasingFn,
      travelEasingFn,
      travelEasingFn,
      token);
    var spinEasingFn = EasingFunctions.FromName(Settings.EggSpinupEasingFunctionName);
    var spinner = egg.GetComponentInChildren<LocalClockSpinner>();
    var spin = Tasks.Tween(
      0,
      Settings.MaxEggSpinSpeed,
      LocalClock,
      Settings.EggTravelDuration.Ticks,
      spinEasingFn,
      f => spinner.DegreesPerSecond = f,
      token);
    var vibrationEasingFunction = EasingFunctions.FromName(Settings.EggVibrationEasingFunctionName);
    var vibrator = egg.GetComponent<Vibrator>();
    vibrator.Vibrate(Vector3.up, Settings.EggTravelDuration.Ticks, 0, 20);
    var vibrate = Tasks.Tween(
      0,
      Settings.MaxEggVibration,
      LocalClock,
      Settings.EggTravelDuration.Ticks,
      vibrationEasingFunction,
      f => {
        vibrator.Axis = Random.onUnitSphere;
        vibrator.Amplitude = f;
      },
      token);

    var normalTravel = UniTask.WhenAll(travel, spin, vibrate);
    var outcome = await UniTask.WhenAny(normalTravel, eggCollision);
    // Normal Travel Occured
    if (outcome == 0) {
      var halfSpread = Settings.DragonSpreadAngle / 2f;
      for (var i = 0; i < Settings.DragonCount; i++) {
        var angle = Mathf.Lerp(-halfSpread, halfSpread, (float)i / (Settings.DragonCount - 1));
        var offsetRotation = Quaternion.Euler(0, angle, 0);
        var direction = offsetRotation * (rotation * Vector3.forward);
        var dragon = Instantiate(Settings.DragonPrefab, egg.transform.position, offsetRotation * rotation, transform);
        var velocity = Settings.DragonTravelSpeed * direction;
        dragon.GetComponent<Rigidbody>().AddForce(velocity, ForceMode.VelocityChange);
        dragon.GetComponent<FireDropper>().Settings = Settings;
      }
    }
    Instantiate(Settings.ExplosionPrefab, egg.transform.position, rotation, transform);
    var hitCount = Physics.OverlapSphereNonAlloc(egg.transform.position, Settings.ExplosionRadius, Colliders);
    for (var i = 0; i < hitCount; i++) {
      var collider = Colliders[i];
      if (collider.TryGetComponent(out SpellAffected spellAffected)) {
        Debug.Log($"Hit {collider.name}", collider.gameObject);
        var delta = collider.transform.position-egg.transform.position;
        var direction = delta.normalized;
        spellAffected.Push(Settings.ExplosionKnockback / LocalClock.DeltaTime() * direction);
      }
    }
    Destroy(egg.gameObject);
    await Tasks.Delay(60 * 2, LocalClock, token);
    Destroy(gameObject);
  }
}