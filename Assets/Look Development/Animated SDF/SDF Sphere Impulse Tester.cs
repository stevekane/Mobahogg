using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SDFSphere))]
class SDFSphereImpulseTester : MonoBehaviour
{
  [SerializeField] float ImpulsePeriod = 1;
  [SerializeField] float Frequency = 1;
  [SerializeField] float StretchDecayRate = 1;
  [SerializeField, Range(0.1f, 1f)] float StretchFraction = 0.5f;
  float CurrentStretchFraction;
  float ImpulseTime;

  IEnumerator Start()
  {
    while (true)
    {
      CurrentStretchFraction = StretchFraction;
      ImpulseTime = Time.time;
      yield return new WaitForSeconds(ImpulsePeriod);
    }
  }

  // Want to improve this so that it always begins going negative
  // this means we need to not use raw Time but rather time since
  // the impulse occurredand perhaps the cheapest thing to do is
  // offset it by negation or shifting the offset
  void Update()
  {
    var sphere = GetComponent<SDFSphere>();
    var tImpulse = Time.time - ImpulseTime;
    // negate the sin function so it always begins with compression
    sphere.StretchFraction = 1.0f + CurrentStretchFraction * -Mathf.Sin(Frequency * tImpulse);
    CurrentStretchFraction = Mathf.Max(0, CurrentStretchFraction - Time.deltaTime * StretchDecayRate);
  }
}