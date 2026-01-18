
/*
This class wraps around a raw underlying list and provides a single method called
append which will add neighbors to the underlying list. This is handed to Neighborhoods
and allows them to add neighbors to the underlying list while having no other access to it.
*/
using System.Collections.Generic;

public readonly struct AppendOnly<T>
{
  readonly List<T> list;
  public AppendOnly(List<T> underlyingList) => list = underlyingList;
  public void Append(T t) => list.Add(t);
}