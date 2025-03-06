using UnityEngine;

[CreateAssetMenu(fileName = "Frame Behaviors Preview Config", menuName = "Frame Behaviors/Preview Config")]
public class FrameBehaviorsPreviewConfig : ScriptableObject {
  public static FrameBehaviorsPreviewConfig Instance;

  public GameObject FloorPrefab;

  [ContextMenu("Set As Instance")]
  void SetSingletonInstance() {
    Instance = this;
  }

  void OnEnable() {
    if (Instance != null) {
      Debug.LogWarning($"Project should have exactly one instance of FrameBehaviorsPreviewConfig.");
    }
    Instance = this;
  }

  void OnDisable() {
    if (Instance == this)
      Instance = null;
  }
}