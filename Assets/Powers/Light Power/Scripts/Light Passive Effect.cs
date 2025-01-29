using UnityEngine;

public class LightPassiveEffect : Effect {
  [SerializeField] LightPowerSettings Settings;

  int Frame = 0;

  void FixedUpdate() {
    if (Frame >= Settings.ChimeFrameCooldown.Ticks) {
      var distance = Random.Range(Settings.ChimeMinSpawnRadius, Settings.ChimeMaxSpawnRadius);
      var direction = Random.onUnitSphere.XZ();
      var position = EffectManager.transform.position + distance * direction;
      var terrainResult = TerrainManager.Instance.SamplePoint(position);
      if (terrainResult.HasValue) {
        Instantiate(Settings.ChimePrefab, terrainResult.Value.Point, Quaternion.identity);
        Frame = 0;
      }
    } else {
      Frame++;
    }
  }
}