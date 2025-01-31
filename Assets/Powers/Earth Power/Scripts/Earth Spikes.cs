using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

public class EarthSpikes : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] VisualEffect VisualEffect;
  [SerializeField] float Distance = 10;

  void Start() {
    Run(this.destroyCancellationToken).Forget();
  }

  async UniTask Run(CancellationToken token) {
    var start = transform.position;
    var end = transform.position + Distance * transform.forward;
    var frames = 3 * 60;
    await Tasks.EveryFrame(
      frames,
      LocalClock,
      token,
      frame => {
        var position = Vector3.Lerp(start, end, (float)frame/frames);
        var terrainSample = TerrainManager.Instance.SamplePoint(position);
        if (terrainSample.HasValue) {
          VisualEffect.SetVector3("Position", terrainSample.Value.Point);
          VisualEffect.SendEvent("SpawnSpike");
        }
      });
    Destroy(gameObject);
  }
}