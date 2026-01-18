using UnityEngine;

public interface INavigationNeighborhood
{
  public NavigationTag Tag { get; set; }
  public void AppendNeighbors(Vector3 from, AppendOnly<Neighbor> neighbors);
}