using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public abstract class FrameBehavior {
  [Min(0)]
  public int StartFrame = 0;
  public int EndFrame = 1;
  public bool Starting(int frame) => frame == StartFrame;
  public bool Ending(int frame) => frame == EndFrame;
  public bool Active(int frame) => frame >= StartFrame && frame <= EndFrame;

  public virtual string Name => "Frame Behavior";
  public virtual void OnStart(GameObject runner, GameObject owner) {}
  public virtual void OnEnd(GameObject runner, GameObject owner) {}
  public virtual void OnUpdate(GameObject runner, GameObject owner) {}
  public virtual FrameBehavior ShallowClone() => MemberwiseClone() as FrameBehavior;
}

public class FrameBehaviorExecution {
  public List<FrameBehavior> FrameBehaviors = new();
  public FrameBehaviorExecution(List<FrameBehavior> frameBehaviors) {
    foreach (var frameBehavior in frameBehaviors) {
      FrameBehaviors.Add(frameBehavior.ShallowClone());
    }
  }

}