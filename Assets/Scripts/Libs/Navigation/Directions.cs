using UnityEngine;

public static class Directions
{
  public static readonly Vector3[] Octal = new[]
  {
    Vector3.forward,
    Vector3.right,
    Vector3.back,
    Vector3.left,
    Vector3.forward + Vector3.right,
    Vector3.right + Vector3.back,
    Vector3.back + Vector3.left,
    Vector3.left + Vector3.forward
  };
}