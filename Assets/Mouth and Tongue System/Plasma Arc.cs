using UnityEngine;
using UnityEngine.VFX;

public class PlasmaArc : MonoBehaviour
{
  static readonly int p0Hash = Shader.PropertyToID("P0");
  static readonly int p1Hash = Shader.PropertyToID("P1");
  static readonly int p2Hash = Shader.PropertyToID("P2");
  static readonly int p3Hash = Shader.PropertyToID("P3");

  [SerializeField] VisualEffect VisualEffect;

  public void SetPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
  {
    VisualEffect.SetVector3(p0Hash, p0);
    VisualEffect.SetVector3(p1Hash, p1);
    VisualEffect.SetVector3(p2Hash, p2);
    VisualEffect.SetVector3(p3Hash, p3);
  }
}