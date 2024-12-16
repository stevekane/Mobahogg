using System.Threading;
using Cysharp.Threading.Tasks;
using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class SpellFlower : MonoBehaviour {
  [SerializeField] AudioClip OpenAudioClip;
  [SerializeField] int OpenDuration = 60 * 5;
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;

  bool Open;

  void Start() {
    SpellFlowerManager.Active.Flowers.Add(this);
  }

  void OnDestroy() {
    SpellFlowerManager.Active.Flowers.Remove(this);
  }

  [ContextMenu("Open")]
  void TestOpen() {
    OnOpen(this.destroyCancellationToken).Forget();
  }

  async UniTask OnOpen(CancellationToken token) {
    Open = true;
    Animator.SetTrigger("Open");
    // AudioManager.Instance.PlaySoundWithCooldown(OpenAudioClip);
    await Tasks.Delay(OpenDuration, LocalClock, token);
    Destroy(gameObject);
  }

  void FixedUpdate() {
    if (!Open && Health.CurrentValue <= 0) {
      SpellFlowerManager.Active.OnFlowerOpen(this);
      OnOpen(this.destroyCancellationToken).Forget();
    }
  }
}