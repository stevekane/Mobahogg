using UnityEngine;

public class Player : MonoBehaviour {
  public static Player Get() => FindFirstObjectByType<Player>();

  void Awake() {
    PlayerManager.Instance?.RegisterPlayer(this);
  }
  void OnDestroy() {
    PlayerManager.Instance?.UnregisterPlayer(this);
  }
}