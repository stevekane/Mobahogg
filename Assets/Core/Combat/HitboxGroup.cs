using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Manages hits from a single attack registered by 1 or more Hitboxes. Hitboxes earlier in the list
// have priority, so that only the first highest priority Hitbox registers for a given attack.
[DefaultExecutionOrder(ScriptExecutionGroups.Early)]
public class HitboxGroup : MonoBehaviour {
  [Tooltip("Hitboxes earlier in the list have priority")]
  [SerializeField] Hitbox[] Hitboxes;
  List<(Hitbox, Hurtbox)> Hits = new();
  bool DidProcessHit = false;

  void Awake() {
    Hitboxes.ForEach(hb => hb.HitboxGroup = this);
    StartCoroutine(LateFixedUpdateHack());
  }

  void OnEnable() {
    Hits.Clear();
    DidProcessHit = false;
    Hitboxes.ForEach(hb => hb.CollisionEnabled = true);
  }
  void OnDisable() {
    Hitboxes.ForEach(hb => hb.CollisionEnabled = false);
  }

  public void OnHit(Hitbox attacker, Hurtbox victim) {
    Debug.Assert(Hitboxes.Contains(attacker));
    Hits.Add((attacker, victim));
  }

  void ProcessHit(Hitbox attacker, Hurtbox victim) {
    if (DidProcessHit) {
      //Debug.Log($"HIT alreadyprocessed {attacker.Owner.name}/{attacker.name} vs {victim.Owner.name}/{victim.name} tick={Timeval.TickCount}");
      return;
    }
    Debug.Log($"HIT process {attacker.Owner.name}/{attacker.name} vs {victim.Owner.name}/{victim.name} tick={Timeval.TickCount}");
    DidProcessHit = true;
  }

  void LateFixedUpdate() {
    if (Hits.Count == 0) return;
    var highestPriorityPair = Hits
      .Select(hit => (Hitboxes.IndexOf(hit.Item1), hit)) // list of hits with index of hb in Hitboxes
      .Aggregate((best, next) => best.Item1 < next.Item1 ? best : next); // (index, hit) of highest priority hit.
    var hit = highestPriorityPair.Item2;
    ProcessHit(hit.Item1, hit.Item2);
    Hits.Clear();
  }

  // FixedUpdate runs BEFORE physics collisions are resolved.
  // WaitForFixedUpdate yields AFTER.
  // We use this to simulate a LateFixedUpdate step.
  IEnumerator LateFixedUpdateHack() {
    while (true) {
      yield return new WaitForFixedUpdate();
      LateFixedUpdate();
    }
  }
}