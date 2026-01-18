using System.Collections.Generic;

public readonly struct AppendOnly<T>
{
  readonly List<T> list;
  public AppendOnly(List<T> underlyingList) => list = underlyingList;
  public void Append(T t) => list.Add(t);
}