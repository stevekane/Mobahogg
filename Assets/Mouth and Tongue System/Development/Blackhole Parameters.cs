using UnityEngine;

[CreateAssetMenu(fileName = "Blackhole Paramaters", menuName = "Blackholes / Parameters")]
public class BlackholeParameters : ScriptableObject {
  public float MoveSpeed = 10;
  public EasingFunctions.EasingFunctionName OrbitTransitionEasingFunctionName;
  public Timeval OrbitTransitionDuration = Timeval.FromMillis(250);
  public Timeval OrbitDuration = Timeval.FromSeconds(5);
  public float MinOrbitOffset = 10;
  public float OrbitOffset = 2.5f;
  public float InitialOrbitSpeed = 50;
  public float OrbitAcceleration = 25;
  public float SpiralInRate = 3;
}
