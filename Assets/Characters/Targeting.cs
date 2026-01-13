using System.Collections.Generic;
using UnityEngine;
namespace Characters
{
  static class Targeting
  {
    public static Transform BestTarget(this Transform from, IEnumerable<MonoBehaviour> candidates)
    {
      Transform target = null;
      foreach (var candidate in candidates)
      {
        if (target == null)
        {
          target = candidate.transform;
        }
        else
        {
          var c = candidate.transform.position.XZ();
          var f = from.position.XZ();
          var t = target.transform.position.XZ();
          if (Vector3.SqrMagnitude(c - f) < Vector3.SqrMagnitude(t - f))
          {
            target = candidate.transform;
          }
        }
      }
      return target;
    }

  }
}