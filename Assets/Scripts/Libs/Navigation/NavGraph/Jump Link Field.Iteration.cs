public sealed partial class JumpLinkField
{
  public int FirstEdgeIndex(int fromCellIndex)
  {
    if ((uint)fromCellIndex >= (uint)_head.Length) return -1;
    return _head[fromCellIndex];
  }

  public int NextEdgeIndex(int edgeIndex)
  {
    if ((uint)edgeIndex >= (uint)_next.Length) return -1;
    return _next[edgeIndex];
  }

  public JumpEdge GetEdge(int edgeIndex)
  {
    return _edges[edgeIndex];
  }
}