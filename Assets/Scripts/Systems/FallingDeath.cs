using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Systems)]
public class FallingDeath : MonoBehaviour {
  [SerializeField] Health Health;

  void FixedUpdate() {
    if (transform.position.y <= -10) {
      Health.Change(-1000);
    }
  }
}