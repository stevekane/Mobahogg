using State;
using UnityEngine;

public class PlayerDeath : MonoBehaviour {
  [SerializeField] Health Health;

  void Start() {
    Health.OnChange.Listen(NotifyManagers);
  }

  void OnDestroy() {
    Health.OnChange.Unlisten(NotifyManagers);
  }

  void NotifyManagers() {
    if (Health.CurrentValue <= 0) {
      CreepManager.Active.OnOwnerDeath(GetComponent<CreepOwner>());
      LivesManager.Active.OnPlayerDeath(GetComponent<Player>());
    }
  }
}