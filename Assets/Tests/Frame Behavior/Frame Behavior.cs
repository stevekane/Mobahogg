using System;
using UnityEngine;

[Serializable]
public abstract class FrameBehavior {
  [Min(0)]
  public int StartFrame = 0;
  public int EndFrame = 1;
  public bool Starting(int frame) => frame == StartFrame;
  public bool Ending(int frame) => frame == EndFrame;
  public bool Active(int frame) => frame >= StartFrame && frame <= EndFrame;

  public virtual string Name => "Frame Behavior";
  public virtual void OnStart(GameObject runner, GameObject owner, ref FrameBehavior behavior) {}
  public virtual void OnEnd(GameObject runner, GameObject owner, ref FrameBehavior behavior) {}
  public virtual void OnUpdate(GameObject runner, GameObject owner, ref FrameBehavior behavior) {}
}