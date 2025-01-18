using System.Collections.Generic;
using UnityEngine;

namespace AimAssist {
  [DefaultExecutionOrder((int)ExecutionGroups.Managers)]
  public class AimAssistManager : SingletonBehavior<AimAssistManager> {
    public List<AimAssistTarget> Targets = new();

    RaycastHit[] RaycastHits = new RaycastHit[256];

    public AimAssistTarget BestTarget(Transform assist, AimAssistQuery query) {
      AimAssistTarget bestCandidate = null;
      var bestCandidateDistance = float.MaxValue;
      foreach (var target in Targets) {
        var p0 = assist.position;
        var p1 = target.transform.position;
        var delta = p1-p0;
        var distance = delta.magnitude;
        var toTarget = delta.normalized;
        var inRange = distance <= query.MaxDistance;
        var angle = Vector3.Angle(assist.transform.forward, toTarget);
        var inView = angle <= query.MaxAngle;
        // TODO: There is probably a smarter approach here. Maybe pass eyeOffset in the query
        // then cast the ray at the collider's center or something... not totally reliable
        // for non-normal colliders but maybe enough?
        // Also consider jumping characters... what should happen in these cases?
        // Could add additional constraints to the query such as "similar altitude"
        var eyeOffset = 0.25f * Vector3.up;
        var ray = new Ray(assist.position + eyeOffset, toTarget);
        if (target.OwnedBy(assist.gameObject))
          continue;
        if (!inRange)
          continue;
        if (!inView)
          continue;
        // TODO: Should we check only the first thing hit? Probably not?
        var hitCount = Physics.RaycastNonAlloc(ray, RaycastHits, distance, query.LayerMask, query.TriggerInteraction);
        for (var i = 0; i < hitCount; i++) {
          var hit = RaycastHits[i];
          if (hit.transform.TryGetComponent(out AimAssistTarget targetHit) && !targetHit.OwnedBy(assist.gameObject)) {
            if (!bestCandidate || distance < bestCandidateDistance) {
              bestCandidate = targetHit;
              bestCandidateDistance = distance;
            }
          }
        }
      }
      return bestCandidate;
    }
  }
}