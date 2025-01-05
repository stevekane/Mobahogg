using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EarthSpell : Spell {
  [SerializeField] EarthSpellSettings Settings;
  [SerializeField] LocalClock LocalClock;

  List<GameObject> Rocks = new(512);
  List<GameObject> Spikes = new(512);

  // Assumes you hand it a "forward" vector. This code is dogshit
  public static Vector3 PerturbVector(Vector3 inputVector, float minAngle, float maxAngle) {
    Vector3 normalizedVector = inputVector.normalized;
    Vector3 randomAxis = Vector3.right;
    float randomAngle = Random.Range(minAngle, maxAngle);
    return Quaternion.AngleAxis(randomAngle, randomAxis) * normalizedVector;
  }

  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    var start = position;
    start.y -= 1;
    var end = start + rotation * (Settings.BallTravelDistance * Vector3.forward);
    Run(start, end, this.destroyCancellationToken).Forget();
  }

  async UniTask Run(Vector3 start, Vector3 end, CancellationToken token) {
    var earthBall = Instantiate(Settings.EarthBallPrefab, start, Quaternion.LookRotation(end-start), transform);
    var travelFramesF = (float)Settings.TravelFrames;
    var forward = (end-start).normalized;
    var right = Vector3.Cross(forward, Vector3.up);
    var rotation = Quaternion.LookRotation(forward);
    var position = start;
    var rb = earthBall.GetComponent<Rigidbody>();
    for (var i = 0; i < Settings.TravelFrames; i+=LocalClock.DeltaFrames()) {
      var p = Vector3.Lerp(start, end, (float)i / travelFramesF);
      var terrainSample = TerrainManager.Instance.SamplePoint(p);
      if (!terrainSample.HasValue)
        break;
      var terrainPoint = terrainSample.Value.Point;
      SpawnRock(terrainPoint, rotation, right);
      rb.MovePosition(terrainPoint);
      await Tasks.Delay(1, LocalClock, token);
    }
    Destroy(earthBall.gameObject);
    Rocks.ForEach(r => SpawnSpike(r.transform.position, r.transform.rotation, right));
    Rocks.ForEach(Destroy);
    CameraManager.Instance.Shake(Settings.CameraShakeIntensity);
    foreach (var player in LivesManager.Active.Players) {
      var distance = PhysicsUtils.DistanceFromLine(start.XZ(), end.XZ(), player.transform.position.XZ());
      if (distance <= Settings.MaxDamageDistance && player.TryGetComponent(out SpellAffected spellAffected)) {
        spellAffected.ChangeHealth(Settings.HealthDelta);
        spellAffected.Knockback(Settings.KnockbackStrength * Vector3.up);
      }
    }
    await Tasks.Delay(Settings.LingerFrames, LocalClock, token);
    var frames = 30;
    for (var i = 0; i < frames; i++) {
      foreach (var spike in Spikes) {
        spike.transform.Translate(spike.transform.localScale.y / (float)frames * -spike.transform.up, Space.World);
      }
      await Tasks.Delay(1, LocalClock, token);
    }
    Destroy(gameObject);
  }

  void SpawnRock(Vector3 position, Quaternion rotation, Vector3 right) {
    var offset = Random.Range(-Settings.RockMaxLateralOffset, Settings.RockMaxLateralOffset) * right;
    var rock = Instantiate(Settings.RockPrefab, offset+position, rotation, transform);
    var sx = Random.Range(Settings.RockMinSize, Settings.RockMaxSize);
    var sy = Random.Range(Settings.RockMinSize, Settings.RockMaxSize);
    var sz = Random.Range(Settings.RockMinSize, Settings.RockMaxSize);
    rock.transform.localScale = new Vector3(sx, sy, sz);
    var vibrationAxis = Random.onUnitSphere;
    rock.GetComponent<Vibrator>().Vibrate(vibrationAxis, 120, Settings.RockJitterAmplitude, Settings.RockJitterFrequency);
    Rocks.Add(rock);
  }

  void SpawnSpike(Vector3 position, Quaternion rotation, Vector3 right) {
    var spike = Instantiate(Settings.SpikePrefab, position, rotation, transform);
    var sxz = Random.Range(Settings.SpikeMinThickness, Settings.SpikeMaxThickness);
    var sy = Random.Range(Settings.SpikeMinLength, Settings.SpikeMaxLength);
    var direction = PerturbVector(Vector3.forward, -Settings.SpikeMaxTiltAngle, Settings.SpikeMaxTiltAngle);
    direction = Quaternion.AngleAxis(25, right) * direction;
    spike.transform.localScale = new Vector3(sxz, sy, sxz);
    spike.transform.rotation = Quaternion.LookRotation(direction);
    Instantiate(Settings.RockSprayPrefab, position, rotation, transform);
    Spikes.Add(spike);
  }
}