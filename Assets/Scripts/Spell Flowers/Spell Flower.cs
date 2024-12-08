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

  void OnHurt(Combatant attacker) {
    Health.Change(-1);
  }

  [ContextMenu("Open")]
  void TestOpen() {
    OnOpen(this.destroyCancellationToken).Forget();
  }

  async UniTask OnOpen(CancellationToken token) {
    Open = true;
    Animator.SetTrigger("Open");
    // AudioManager.Instance.PlaySoundWithCooldown(OpenAudioClip);
    await Tasks.DelayWith(OpenDuration, LocalClock, token);
    // TODO: Implement some kind of fadeaway
    Destroy(gameObject);
  }

  void FixedUpdate() {
    if (!Open && Health.Value <= 0) {
      OnOpen(this.destroyCancellationToken).Forget();
    }
  }
}