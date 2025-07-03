using UnityEngine;

public class TongueHealthMeter : MonoBehaviour
{
  [SerializeField] GameObject[] Slices;

  public void SetHealth(int health)
  {
    for (var i = 0; i < Slices.Length; i++)
    {
      Slices[i].SetActive(i < health);
    }
  }
}