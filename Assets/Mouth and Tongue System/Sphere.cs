using System;
using System.Collections.Generic;
using Melee;
using UnityEngine;

[Serializable]
class SphereImpulse
{
  public int Tick;
  public int Ticks;
  public float TotalDistance;
  public float CurrentDistance;
  public float CurrentDelta;
  public Vector3 Direction;
  Func<float, float> EasingFunction;

  public SphereImpulse(
  Vector3 direction,
  float distance,
  int ticks,
  EasingFunctions.EasingFunctionName easingFunctionName)
  {
    Tick = 0;
    Ticks = ticks;
    TotalDistance = distance;
    CurrentDistance = 0;
    CurrentDelta = 0;
    Direction = direction.normalized;
    EasingFunction = EasingFunctions.FromName(easingFunctionName);
  }

  public bool IsComplete => Tick > Ticks;

  public Vector3 Delta => CurrentDelta * Direction;

  public void Step() {
    var i = (float)Tick / Ticks;
    var previousDistance = CurrentDistance;
    CurrentDistance = Mathf.Lerp(0, TotalDistance, EasingFunction(i));
    CurrentDelta = CurrentDistance - previousDistance;
    Tick++;
  }
}

class Sphere : MonoBehaviour
{
  public GameObject ImpactVFXPrefab;
  public float ImpactStrength = 1000;
  public float Radius = 1.5f;
  public Vector3 DirectVelocity;
  public List<SphereImpulse> Impulses = new(16);

  [Header("Response To Being Hit")]
  public float StrikeImpulseDistance = 5;
  public Timeval StrikeImpulseDuration = Timeval.FromTicks(10);
  public EasingFunctions.EasingFunctionName StrikeImpulseEasingFunctionName;
  public Vector3 TugOfWarAxis = Vector3.right;

  public void OnHurt(MeleeAttackEvent meleeAttackEvent)
  {
    Impulses.Add(
      new(
        Vector3.Dot(TugOfWarAxis, meleeAttackEvent.ToVictim.XZ().normalized) * TugOfWarAxis,
        StrikeImpulseDistance,
        StrikeImpulseDuration.Ticks,
        StrikeImpulseEasingFunctionName));
  }

  void FixedUpdate()
  {
    var impulseMotion = Impulses.Sum((impulse) => impulse.Delta);
    var directMotion = Time.fixedDeltaTime * DirectVelocity;
    GetComponent<Rigidbody>().MovePosition(transform.position + directMotion + impulseMotion);
    Impulses.ForEach(i => i.Step());
    Impulses.RemoveAll(i => i.IsComplete);
    DirectVelocity = Vector3.zero;
  }
}