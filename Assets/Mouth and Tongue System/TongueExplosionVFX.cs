using System.Collections;
using UnityEngine;

class TongueExplosionVFX : MonoBehaviour
{
  [SerializeField] GameObject ExplosionPrefab;
  [SerializeField] Timeval Duration = Timeval.FromTicks(20);
  [SerializeField] int TickExplosionPeriod = 4;

  public Vector3 Destination;

  IEnumerator Start()
  {
    var ticks = Duration.Ticks;
    var deltaPosition = (1f / ticks) * (Destination - transform.position);
    for (var i = 0; i < ticks; i++)
    {
      transform.position += deltaPosition;
      if (i % TickExplosionPeriod  == 0)
      {
        Destroy(Instantiate(ExplosionPrefab, transform.position, transform.rotation), 3);
      }
      yield return new WaitForFixedUpdate();
    }
  }
}