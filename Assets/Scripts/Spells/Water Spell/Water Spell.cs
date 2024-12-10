using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaterSpell : Spell {
  [SerializeField] GameObject WaterBallPrefab;
  [SerializeField] GameObject BlizzardPrefab;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Vector3 Delta = new(0, 5, 10);
  [SerializeField] int TravelFrames = 60;
  [SerializeField] int BlizzardFrames = 60 * 5;

  public override void Cast(Vector3 position, Quaternion rotation, Player owner) {
    var start = position;
    var end = position + rotation * Delta;
    Run(start, end, this.destroyCancellationToken).Forget();
  }

  async UniTask Run(Vector3 start, Vector3 end, CancellationToken token) {
    var waterBall = Instantiate(WaterBallPrefab, start, Quaternion.identity, transform);
    var travelFramesF = (float)TravelFrames;
    for (var i = 0; i < TravelFrames; i++) {
      var rb = waterBall.GetComponent<Rigidbody>();
      var position = Vector3.Lerp(start, end, (float)i / travelFramesF);
      rb.MovePosition(position);
      await Tasks.DelayWith(1, LocalClock, token);
    }
    Destroy(waterBall.gameObject);
    Instantiate(BlizzardPrefab, end, Quaternion.identity, transform);
    await Tasks.DelayWith(BlizzardFrames, LocalClock, token);
    Destroy(gameObject);
  }
}