using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackholeSpawner : MonoBehaviour
{
  [SerializeField] Transform Target;
  [SerializeField] float MoveSpeed = 2;

  List<GameObject> Blackholes = new(15);

  IEnumerator Start()
  {
    while (true)
    {
      if (Blackholes.Count < 15)
      {
        var blackhole = new GameObject("Blackhole");
        var initialPosition = Random.onUnitSphere;
        initialPosition *= 30;
        initialPosition.y = Mathf.Abs(initialPosition.y);
        blackhole.transform.position = initialPosition;
        blackhole.AddComponent<SDFSphere>();
        Blackholes.Add(blackhole);
      }
      yield return new WaitForSeconds(1);
    }
  }

  void FixedUpdate()
  {
    for (var i = Blackholes.Count - 1; i >= 0; i--)
    {
      var blackhole = Blackholes[i];
      if (Vector3.Distance(Target.position, blackhole.transform.position) < 0.1)
      {
        Debug.Log("Destroyed");
        Destroy(blackhole);
        Blackholes.RemoveAt(i);
      }
      else
      {
        blackhole.transform.position = Vector3.MoveTowards(
          current: blackhole.transform.position,
          target: Target.transform.position,
          MoveSpeed * Time.fixedDeltaTime);
      }
    }
  }
}