using System.Collections;
using UnityEngine;

public static class CoroutineCombinators
{
  public static IEnumerator BothCoroutines(
  this MonoBehaviour host,
  IEnumerator a,
  IEnumerator b)
  {
    var aDone = false;
    var bDone = false;
    IEnumerator AWrapper()
    {
      yield return a;
      aDone = true;
    }
    IEnumerator BWrapper()
    {
      yield return b;
      bDone = true;
    }
    host.StartCoroutine(AWrapper());
    host.StartCoroutine(BWrapper());
    yield return new WaitUntil(() => aDone && bDone);
  }

  public static IEnumerator FirstCoroutine(
  this MonoBehaviour host,
  IEnumerator a,
  IEnumerator b)
  {
    var aDone = false;
    var bDone = false;
    IEnumerator AWrapper()
    {
      yield return a;
      aDone = true;
    }
    IEnumerator BWrapper()
    {
      yield return b;
      bDone = true;
    }
    var aRoutine = host.StartCoroutine(AWrapper());
    var bRoutine = host.StartCoroutine(BWrapper());
    yield return new WaitUntil(() => aDone || bDone);
    host.StopCoroutine(aRoutine);
    host.StopCoroutine(bRoutine);
  }

}