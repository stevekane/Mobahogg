using Animation;
using UnityEngine;
using UnityEngine.Playables;

public interface IAnimationSlot {
  public SlotBehavior SlotBehavior { get; }
  public PlayableGraph PlayableGraph { get; }
}

public abstract class AnimationGraph : MonoBehaviour, IAnimationSlot {
  public abstract SlotBehavior SlotBehavior { get; }
  public abstract PlayableGraph PlayableGraph { get; }
}