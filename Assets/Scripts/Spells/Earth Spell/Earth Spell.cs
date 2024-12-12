using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/*
Keep track of how far the ball moves through the earth.
As it moves through the earth, it should create rock instances
along the path. It aims to place these rocks with a certain
density.

This density is linear along the traced out path.
Everytime it's appropriate to lay down new rocks, do so. keep track
of where you are along the path and each frame determine how many rocks
to place.
*/
public class EarthSpell : Spell {
  [SerializeField] GameObject EarthBallPrefab;
  [SerializeField] GameObject RockPrefab;
  [SerializeField] GameObject SpikePrefab;
  [SerializeField] GameObject RockSprayPrefab;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] float TravelDistance = 20f;
  [SerializeField] int TravelFrames = 60;
  [SerializeField] int LingerFrames = 60;
  [SerializeField] float Density = 1;

  List<GameObject> Rocks = new(512);
  List<GameObject> Spikes = new(512);

  // Assumes you hand it a "forward" vector. This code is dogshit
  public static Vector3 PerturbVector(Vector3 inputVector, float minAngle, float maxAngle) {
    Vector3 normalizedVector = inputVector.normalized;
    Vector3 randomAxis = Vector3.right;
    float randomAngle = Random.Range(minAngle, maxAngle);
    return Quaternion.AngleAxis(randomAngle, randomAxis) * normalizedVector;
  }

  // TODO: Need to eventually handle height variation. Skip for v1
  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    var start = position;
    // TODO: Hack to make this run through the earth...do this better
    start.y -= 1;
    var end = start + rotation * (TravelDistance * Vector3.forward);
    Run(start, end, this.destroyCancellationToken).Forget();
  }

  async UniTask Run(Vector3 start, Vector3 end, CancellationToken token) {
    var earthBall = Instantiate(EarthBallPrefab, start, Quaternion.LookRotation(end-start), transform);
    var travelFramesF = (float)TravelFrames;
    var direction = (end-start).normalized;
    var deposits = TravelDistance / Density;
    var rotation = Quaternion.LookRotation(direction);
    var densityDistance = TravelDistance / deposits;
    var positionDelta = densityDistance * direction;
    var position = start;
    // Place all rocks inactive
    for (var i = 0; i < deposits; i++) {
      var rock = Instantiate(RockPrefab, position, rotation, transform);
      var sx = Random.Range(0.1f, 0.25f);
      var sy = Random.Range(0.1f, 0.25f);
      var sz = Random.Range(0.1f, 0.25f);
      rock.transform.localScale = new Vector3(sx, sy, sz);
      rock.transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
      rock.SetActive(false);
      Rocks.Add(rock);
      position += positionDelta;
    }
    // Place all spikes inactive
    position = start;
    for (var i = 0; i < deposits; i++) {
      var spike = Instantiate(SpikePrefab, position, rotation, transform);
      var sxz = Random.Range(0.15f, 0.25f);
      var sy = Random.Range(0.75f, 1.5f);
      spike.transform.localScale = new Vector3(sxz, sy, sxz);
      spike.transform.rotation  = Quaternion.LookRotation(PerturbVector(Vector3.forward, -25, 25));
      spike.SetActive(false);
      Spikes.Add(spike);
      position += positionDelta;
    }
    // Move the earth ball
    // As the earth ball passes a rock location activate it
    // This is crazy levels of stupid lol
    // TODO: Refactor this to avoid the quadratic idiocy of walking this loop over and over...
    // It is definetly more optimal to place the rocks when needed than to
    // run over the list over and over like this
    var rb = earthBall.GetComponent<Rigidbody>();
    for (var i = 0; i < TravelFrames; i++) {
      position = Vector3.Lerp(start, end, (float)i / travelFramesF);
      foreach (var rock in Rocks) {
        var toRock = Vector3.Distance(position, rock.transform.position);
        if (toRock <= .1f) {
          rock.SetActive(true);
        }
      }
      rb.MovePosition(position);
      await Tasks.Delay(1, LocalClock, token);
    }
    // Destroy ball
    Destroy(earthBall.gameObject);
    // Spawn the Spikes
    Rocks.ForEach(s => s.SetActive(false));
    Spikes.ForEach(s => s.SetActive(true));
    Spikes.ForEach(s => Instantiate(RockSprayPrefab, s.transform.position, s.transform.rotation, transform));
    await Tasks.Delay(LingerFrames, LocalClock, token);
    // Clean up
    Destroy(gameObject);
  }
}
