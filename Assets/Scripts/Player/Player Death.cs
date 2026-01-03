using State;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDeath : MonoBehaviour {
  [SerializeField] Health Health;

  void Start() => Health.OnChange.Listen(NotifyManagers);

  void OnDestroy() => Health.OnChange.Unlisten(NotifyManagers);

  void NotifyManagers() {
    if (Health.CurrentValue <= 0) {
      SpawnManager.Active.Respawn(GetComponent<Player>());
    }
  }
}