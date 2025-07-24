using System.Collections;
using UnityEngine;

enum AxisMode
{
  Fixed,
  Random
}

enum ImpulseMode
{
  Fixed,
  Random
}

[RequireComponent(typeof(SDFSphere))]
class SDFSphereImpulseTester : MonoBehaviour
{
  [SerializeField] AxisMode AxisMode;
  [SerializeField] ImpulseMode ImpulseMode;
  [SerializeField] Vector3 Axis = Vector3.up;
  [SerializeField] float ImpulsePeriod = 1;
  [SerializeField] float Frequency = 1;
  [SerializeField] float StretchDecayRate = 1;
  [SerializeField, Range(0.1f, 1f)] float StretchFraction = 0.5f;
  float CurrentStretchFraction;
  float ImpulseTime;

  float WaitPeriod => ImpulseMode switch
  {
    ImpulseMode.Random => Random.Range(0, ImpulsePeriod),
    _ => ImpulsePeriod
  };

  Vector3 StretchAxis => AxisMode switch
  {
    AxisMode.Random => Quaternion.LookRotation(Random.onUnitSphere) * Vector3.forward,
    _ => Axis
  };

  IEnumerator Start()
  {
    while (true)
    {
      CurrentStretchFraction = StretchFraction;
      ImpulseTime = Time.time;
      yield return new WaitForSeconds(WaitPeriod);
    }
  }

  void Update()
  {
    var sphere = GetComponent<SDFSphere>();
    var tImpulse = Time.time - ImpulseTime;
    // negate the sin function so it always begins with compression
    sphere.StretchAxis = StretchAxis;
    sphere.StretchFraction = 1.0f + CurrentStretchFraction * -Mathf.Sin(Frequency * tImpulse);
    CurrentStretchFraction = Mathf.Max(0, CurrentStretchFraction - Time.deltaTime * StretchDecayRate);
  }
}