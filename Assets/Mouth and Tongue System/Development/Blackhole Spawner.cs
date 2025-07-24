using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BlackholeSpawner : MonoBehaviour
{
  [SerializeField] Transform Target;
  [SerializeField] Timeval SpawnPeriod = Timeval.FromMillis(250);
  [SerializeField, InlineEditor] BlackholeParameters Parameters;
  [SerializeField, Range(0, 32-1)] int MinNumberOfLivingBlackholes = 31;
  [SerializeField] float MinRadius = 0.25f;
  [SerializeField] float MaxRadius = 0.75f;
  [SerializeField] float SpawnRadius = 30;

  public float TargetTargetRadius;
  public List<Blackhole> Blackholes = new(32 - 1);

  IEnumerator Start()
  {
    var target = Target.GetComponent<SDFSphere>();
    TargetTargetRadius = target.Radius;
    while (true)
    {
      if (Blackholes.Count < MinNumberOfLivingBlackholes)
      {
        var bhgo = new GameObject("Blackhole");
        var blackhole = bhgo.AddComponent<Blackhole>();
        var sdfSphere = bhgo.AddComponent<SDFSphere>();
        var initialPosition = SpawnRadius * (Quaternion.Euler(0, Random.Range(0, 360), 0) * Vector3.forward);
        blackhole.transform.position = initialPosition;
        blackhole.Target = target;
        blackhole.Paramaters = Parameters;
        blackhole.Spawner = this;
        sdfSphere.Radius = Random.Range(MinRadius, MaxRadius);
      }
      yield return new WaitForSeconds(SpawnPeriod.Seconds);
    }
  }

  // void FixedUpdate()
  // {
  //   const float K = 4f / 3 * Mathf.PI;
  //   const float KInverse = 1f / K;
  //   float Cube(float n) => n * n * n;
  //   float CubeRoot(float n) => Mathf.Pow(n, 1f / 3);
  //   for (var i = Blackholes.Count - 1; i >= 0; i--)
  //   {
  //     var blackhole = Blackholes[i];
  //     var blackholeRadius = blackhole.GetComponent<SDFSphere>().Radius;
  //     if (Vector3.Distance(Target.position, blackhole.transform.position) < 0.25)
  //     {
  //       var blackholeVolume = K * Cube(blackholeRadius);
  //       var targetVolume = K * Cube(TargetTargetRadius);
  //       TargetTargetRadius = CubeRoot(KInverse * (blackholeVolume + targetVolume));
  //       Target.GetComponentInParent<Vibrator>().StartVibrate(
  //         axis: (blackhole.transform.position - Target.transform.position).normalized,
  //         frames: 10,
  //         amplitude: 0.25f,
  //         frequency: 20);
  //       Blackholes.RemoveAt(i);
  //       Destroy(blackhole);
  //     }
  //     else
  //     {
  //       blackhole.transform.position = Vector3.MoveTowards(
  //         current: blackhole.transform.position,
  //         target: Target.transform.position,
  //         MoveSpeed * Time.fixedDeltaTime);
  //     }
  //   }
  //   var targetSDF = Target.GetComponent<SDFSphere>();
  //   targetSDF.Radius = Mathf.MoveTowards(
  //     current: targetSDF.Radius,
  //     target: TargetTargetRadius,
  //     maxDelta: Time.fixedDeltaTime * TargetRadiusGrowthRate);
  // }
}

class Blackhole : MonoBehaviour
{
  public SDFSphere Target;
  public BlackholeParameters Paramaters;
  public BlackholeSpawner Spawner;

  Vector3 OrbitPlane;
  Vector3 OrbitX;
  Vector3 OrbitY;
  float OrbitRadius;
  float OrbitAngle;
  float OrbitTime;

  bool ReachedCriticalRadius {
    get {
      var distance = Vector3.Distance(Target.transform.position, transform.position);
      var minRadius = Target.Radius + Paramaters.MinOrbitOffset;
      return distance <= minRadius;
    }
  }

  Vector3 ToTarget => Target.transform.position - transform.position;

  Vector3 LinearMotionDelta => Time.fixedDeltaTime * Paramaters.MoveSpeed * ToTarget;

  Vector3 OrbitalMotionDelta
  {
    get
    {
      float currentSpeed = Paramaters.InitialOrbitSpeed + Paramaters.OrbitAcceleration * OrbitTime;
      float deltaAngle = Mathf.Rad2Deg * (currentSpeed * Time.fixedDeltaTime / OrbitRadius);
      float nextAngle = OrbitAngle + deltaAngle;
      Vector3 nextPosition =
        Target.transform.position
        + OrbitRadius * (Mathf.Cos(Mathf.Deg2Rad * nextAngle) * OrbitX
                      + Mathf.Sin(Mathf.Deg2Rad * nextAngle) * OrbitY);
      Vector3 delta = nextPosition - transform.position;
      OrbitAngle = nextAngle;
      return delta;
    }
  }

  IEnumerator Start()
  {
    Spawner.Blackholes.Add(this);
    while (!ReachedCriticalRadius)
    {
      transform.position = transform.position + LinearMotionDelta;
      yield return new WaitForFixedUpdate();
    }
    OrbitPlane = Vector3.up;
    OrbitRadius = Paramaters.OrbitOffset;
    OrbitAngle = OrbitalMath.ComputeOrbitalAngle(
      position: transform.position,
      center: Target.transform.position,
      axis: OrbitPlane);
    OrbitX = Vector3.Cross(OrbitPlane, Vector3.up);
    if (OrbitX.sqrMagnitude < 1e-4f)
      OrbitX = Vector3.Cross(OrbitPlane, Vector3.forward);
    OrbitX.Normalize();
    OrbitY = Vector3.Cross(OrbitPlane, OrbitX);
    var totalBlendTicks = Paramaters.OrbitTransitionDuration.Ticks;
    var easingFunction = EasingFunctions.FromName(Paramaters.OrbitTransitionEasingFunctionName);
    for (var i = 0; i < Paramaters.OrbitTransitionDuration.Ticks; i++)
    {
      var interpolant = (float)i / (totalBlendTicks - 1);
      interpolant = easingFunction(interpolant);
      transform.position = transform.position + Vector3.Lerp(LinearMotionDelta, OrbitalMotionDelta, interpolant);
      yield return new WaitForFixedUpdate();
    }
    for (var i = 0; i < Paramaters.OrbitDuration.Ticks; i++)
    {
      OrbitTime += Time.fixedDeltaTime;
      OrbitRadius = Mathf.Max(OrbitRadius - Paramaters.SpiralInRate * Time.fixedDeltaTime, 0.001f);
      transform.position = transform.position + OrbitalMotionDelta;
      yield return new WaitForFixedUpdate();
    }
    Spawner.Blackholes.Remove(this);
    Destroy(gameObject);
  }
}

static class OrbitalMath
{
  public static float ComputeOrbitalAngle(Vector3 position, Vector3 center, Vector3 axis)
  {
    Vector3 fromCenter = position - center;
    Vector3 projected = Vector3.ProjectOnPlane(fromCenter, axis).normalized;
    Vector3 reference = Vector3.Cross(axis, Vector3.up);
    if (reference.sqrMagnitude < 1e-4f)
      reference = Vector3.Cross(axis, Vector3.forward);
    reference = reference.normalized;
    float angle = Vector3.SignedAngle(reference, projected, axis);
    return angle < 0f ? angle + 360f : angle;
  }
}
