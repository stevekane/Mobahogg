using UnityEngine;

public interface INavigationNeighborhood
{
  public NavigationTag Tag { get; set; }
  public int Neighbors(Vector3 from, Neighbor[] buffer, int offset);
}