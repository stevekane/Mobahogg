using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EarthSpell : Spell {
  [SerializeField] GameObject EarthBallPrefab;
  [SerializeField] GameObject RockPrefab;
  [SerializeField] GameObject SpikePrefab;
  [SerializeField] GameObject RockSprayPrefab;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] float TravelDistance = 20f;
  [SerializeField] int TravelFrames = 60;
  [SerializeField] int LingerFrames = 60;

  List<GameObject> Rocks = new(512);

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
    var end = start + rotation * (TravelDistance * Vector3.forward);
    Run(start, end, this.destroyCancellationToken).Forget();
  }

  async UniTask Run(Vector3 start, Vector3 end, CancellationToken token) {
    var earthBall = Instantiate(EarthBallPrefab, start, Quaternion.LookRotation(end-start), transform);
    var travelFramesF = (float)TravelFrames;
    var direction = (end-start).normalized;
    var rotation = Quaternion.LookRotation(direction);
    var position = start;
    var rb = earthBall.GetComponent<Rigidbody>();
    for (var i = 0; i < TravelFrames; i+=LocalClock.DeltaFrames()) {
      var p = Vector3.Lerp(start, end, (float)i / travelFramesF);
      var terrainSample = TerrainManager.Instance.SamplePoint(p);
      if (!terrainSample.HasValue)
        break;
      var terrainPoint = terrainSample.Value.Point;
      SpawnRock(terrainPoint, rotation);
      rb.MovePosition(terrainPoint);
      await Tasks.Delay(1, LocalClock, token);
    }
    Destroy(earthBall.gameObject);
    Rocks.ForEach(SpawnSpike);
    Rocks.ForEach(Destroy);
    await Tasks.Delay(LingerFrames, LocalClock, token);
    Destroy(gameObject);
  }

  void SpawnRock(Vector3 position, Quaternion rotation) {
    var rock = Instantiate(RockPrefab, position, rotation, transform);
    var sx = Random.Range(0.1f, 0.25f);
    var sy = Random.Range(0.1f, 0.25f);
    var sz = Random.Range(0.1f, 0.25f);
    rock.transform.localScale = new Vector3(sx, sy, sz);
    Rocks.Add(rock);
  }

  void SpawnSpike(GameObject go) {
    var spike = Instantiate(SpikePrefab, go.transform.position, go.transform.rotation, transform);
    var sxz = Random.Range(0.15f, 0.25f);
    var sy = Random.Range(0.75f, 1.5f);
    spike.transform.localScale = new Vector3(sxz, sy, sxz);
    spike.transform.rotation  = Quaternion.LookRotation(PerturbVector(Vector3.forward, -25, 25));
    Instantiate(RockSprayPrefab, go.transform.position, go.transform.rotation, transform);
  }
}