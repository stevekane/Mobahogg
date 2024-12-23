using System.Collections.Generic;
using UnityEngine;

/*
Knockback is a list of active forces being applied to this entity.
Each active force has a duration.
These active forces are applied to the Character Controller
Additionally, some reduction in movespeed or something may
also be applied resulting in some reduced ability to control your character.

We may want to support overwriting existing knockback effects.

Think about cases like being hit back and forth.
*/
[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class Knockback : MonoBehaviour {
  [Header("Reads From")]
  [SerializeField] LocalClock LocalClock;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  List<Vector3> Forces = new(16);
  List<int> Durations = new(16);
  List<Vector3> NextForces = new(16);
  List<int> NextDurations = new(16);

  public void Add(Vector3 force, int frames) {
    NextForces.Add(force);
    NextDurations.Add(frames);
  }

  void FixedUpdate() {
    var deltaFrames = LocalClock.DeltaFrames();
    for (var i = 0; i < Forces.Count; i++) {
      CharacterController.Acceleration.Add(Forces[i]);
      Durations[i] = Durations[i]-deltaFrames;
    }
    for (var i = Forces.Count-1; i >= 0; i--) {
      var duration = Durations[i];
      if (duration <= 0) {
        Forces.RemoveAt(i);
        Durations.RemoveAt(i);
      }
    }
    NextForces.ForEach(Forces.Add);
    NextDurations.ForEach(Durations.Add);
    NextForces.Clear();
    NextDurations.Clear();
  }
}