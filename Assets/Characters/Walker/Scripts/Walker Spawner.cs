using UnityEngine;

namespace Characters.Walker
{
  class WalkerSpawner : MonoBehaviour
  {
    [SerializeField] Timeval SpawnCooldown = Timeval.FromSeconds(3);
    [SerializeField] Walker Prefab;

    int Remaining;

    void FixedUpdate()
    {
      Remaining = Mathf.Min(Remaining, SpawnCooldown.Ticks);
      if (Remaining-- <= 0)
      {
        Instantiate(Prefab, transform, false);
        Remaining = SpawnCooldown.Ticks;
      }
    }
  }
}