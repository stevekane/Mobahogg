using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class Flash : MonoBehaviour {
  [SerializeField] Transform[] RendererRoots;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Material FlashMaterial;

  List<Renderer> TargetMeshes = new(8);
  Material[][] OriginalMaterials;
  int RemainingFrames;

  public void Set(int durationFrames) {
    RemainingFrames = Mathf.Max(durationFrames, RemainingFrames);
    foreach (var renderer in TargetMeshes) {
      Material[] flashMaterials = new Material[renderer.materials.Length];
      for (int i = 0; i < renderer.materials.Length; i++) {
        flashMaterials[i] = FlashMaterial;
      }
      renderer.materials = flashMaterials;
    }
  }

  public void TurnOff()
  {
    for (int i = 0; i < TargetMeshes.Count; i++) {
      TargetMeshes[i].materials = OriginalMaterials[i];
    }
  }

  void Awake() {
    foreach (var root in RendererRoots) {
      foreach (var mesh in root.GetComponentsInChildren<Renderer>()) {
        TargetMeshes.Add(mesh);
      }
    }
    OriginalMaterials = new Material[TargetMeshes.Count][];
    for (int i = 0; i < TargetMeshes.Count; i++) {
      OriginalMaterials[i] = TargetMeshes[i].materials;
    }
  }

  void FixedUpdate() {
    if (LocalClock.Parent().Frozen())
      return;

    if (RemainingFrames > 0) {
      RemainingFrames--;
    } else {
      TurnOff();
      RemainingFrames = 0;
    }
  }
}
