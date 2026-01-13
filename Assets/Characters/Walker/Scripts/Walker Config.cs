using UnityEngine;

namespace Characters.Walker
{
  [CreateAssetMenu(menuName = "Characters/Walker/Config")]
  class WalkerConfig : ScriptableObject
  {
    public Timeval SpawnDuration = Timeval.FromSeconds(1);
    public Timeval DyingDuration = Timeval.FromSeconds(1);

    public float MoveSpeed = 6;
    public float TurnSpeed = 180;
  }
}