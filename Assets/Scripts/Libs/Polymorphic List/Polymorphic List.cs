using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PolymorphicList<TBase>
{
  // Unity will serialize the concrete runtime type + data of each element.
  [SerializeReference] List<TBase> items = new();

  public List<TBase> AsList() => items;

  public int Count => items?.Count ?? 0;

  public TBase this[int index]
  {
    get => items[index];
    set => items[index] = value;
  }
}