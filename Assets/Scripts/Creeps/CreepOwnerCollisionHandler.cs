using UnityEngine;
using State;

public class CreepOwnerCollisionHandler : MonoBehaviour {
  [SerializeField] CreepOwner CreepOwner;

  void OnTriggerEnter(Collider other) {
    var dropZone = other.GetComponent<CreepDropZone>();
    var dropZoneTeam = other.GetComponent<Team>();
    var ownerTeam = CreepOwner.GetComponent<Team>();
    if (dropZone != null && dropZoneTeam != null && ownerTeam != null && ownerTeam.TeamType == dropZoneTeam.TeamType) {
      CreepOwner.OnEnterCreepDropZone(dropZone);
    }
  }
}