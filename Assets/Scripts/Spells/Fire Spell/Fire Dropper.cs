using UnityEngine;

public class FireDropper : MonoBehaviour {
  public FireSpellSettings Settings;

  int FramesRemaining;

  void FixedUpdate() {
    if (FramesRemaining <= 0) {

      var fire = Instantiate(Settings.FirePrefab, transform.position-Vector3.up, transform.rotation, null);
      FramesRemaining = Settings.FireDropCooldown.Ticks;
    } else {
      FramesRemaining -= 1;
    }
  }
}