using UnityEngine;

public abstract class FrameBehavior {
  public int StartFrame = 0;
  public int EndFrame = 1;
  public bool Starting(int frame) => frame == StartFrame;
  public bool Ending(int frame) => frame == EndFrame;
  public bool Active(int frame) => frame >= StartFrame && frame <= EndFrame;
}