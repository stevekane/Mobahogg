using UnityEngine;
public abstract class SequenceBehavior : ScriptableObject {
  [Min(0)]
  public int StartFrame;
  [Min(0)]
  public int EndFrame;
  public abstract string Name { get; }
  public abstract void OnStart();
  public abstract void OnFrame();
  public abstract void OnEnd();
}