using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackholeSpawner : MonoBehaviour
{
  [SerializeField] Transform Target;
  [SerializeField] Timeval SpawnPeriod = Timeval.FromMillis(250);
  [SerializeField] float MoveSpeed = 2;
  [SerializeField] float TargetRadiusGrowthRate = 1;
  [SerializeField] float MinRadius = 0.25f;
  [SerializeField] float MaxRadius = 0.75f;

  public float TargetTargetRadius;
  List<GameObject> Blackholes = new(32-1);

  IEnumerator Start()
  {
    TargetTargetRadius = Target.GetComponent<SDFSphere>().Radius;
    while (true)
    {
      if (Blackholes.Count < (32-1))
      {
        var blackhole = new GameObject("Blackhole");
        var initialPosition = Random.onUnitSphere;
        initialPosition.x *= 30;
        initialPosition.z *= 30;
        initialPosition.y = 5;
        blackhole.transform.position = initialPosition;
        var sdfSphere = blackhole.AddComponent<SDFSphere>();
        sdfSphere.Radius = Random.Range(MinRadius, MaxRadius);
        Blackholes.Add(blackhole);
      }
      yield return new WaitForSeconds(SpawnPeriod.Seconds);
    }
  }

  void FixedUpdate()
  {
    const float K = 4f / 3 * Mathf.PI;
    const float KInverse = 1f / K;
    float Cube(float n) => n * n * n;
    float CubeRoot(float n) => Mathf.Pow(n, 1f / 3);
    for (var i = Blackholes.Count - 1; i >= 0; i--)
    {
      var blackhole = Blackholes[i];
      var blackholeRadius = blackhole.GetComponent<SDFSphere>().Radius;
      if (Vector3.Distance(Target.position, blackhole.transform.position) < 0.25)
      {
        var blackholeVolume = K * Cube(blackholeRadius);
        var targetVolume = K * Cube(TargetTargetRadius);
        TargetTargetRadius = CubeRoot(KInverse * (blackholeVolume + targetVolume));
        Target.GetComponentInParent<Vibrator>().StartVibrate(
          axis: (blackhole.transform.position - Target.transform.position).normalized,
          frames: 10,
          amplitude: 0.25f,
          frequency: 20);
        Blackholes.RemoveAt(i);
        Destroy(blackhole);
      }
      else
      {
        blackhole.transform.position = Vector3.MoveTowards(
          current: blackhole.transform.position,
          target: Target.transform.position,
          MoveSpeed * Time.fixedDeltaTime);
      }
    }
    var targetSDF = Target.GetComponent<SDFSphere>();
    targetSDF.Radius = Mathf.MoveTowards(
      current: targetSDF.Radius,
      target: TargetTargetRadius,
      maxDelta: Time.fixedDeltaTime * TargetRadiusGrowthRate);
  }
}