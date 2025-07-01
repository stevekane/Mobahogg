using System.Collections;
using UnityEngine;

public static class CoroutineDelay
{
  public static IEnumerator WaitFixed(
  Timeval timeval)
  {
    for (var i = 0; i < timeval.Ticks; i++)
    {
      yield return new WaitForFixedUpdate();
    }
  }

}