using System.Collections.Generic;

public sealed class MinHeap
{
  readonly List<int> _heap;
  readonly List<float> _priority;

  public int Count => _heap.Count;

  public MinHeap(int capacity)
  {
    _heap = new List<int>(capacity);
    _priority = new List<float>(capacity);
  }

  public void Clear()
  {
    _heap.Clear();
    _priority.Clear();
  }

  public void Push(int nodeIndex, float priority)
  {
    int i = _heap.Count;
    _heap.Add(nodeIndex);
    _priority.Add(priority);

    while (i > 0)
    {
      int p = (i - 1) >> 1;
      if (_priority[p] <= _priority[i]) break;
      Swap(i, p);
      i = p;
    }
  }

  public int PopMin()
  {
    int root = _heap[0];

    int lastIndex = _heap.Count - 1;
    _heap[0] = _heap[lastIndex];
    _priority[0] = _priority[lastIndex];

    _heap.RemoveAt(lastIndex);
    _priority.RemoveAt(lastIndex);

    if (_heap.Count > 0)
    {
      int i = 0;
      while (true)
      {
        int l = (i << 1) + 1;
        if (l >= _heap.Count) break;

        int r = l + 1;
        int s = (r < _heap.Count && _priority[r] < _priority[l]) ? r : l;

        if (_priority[i] <= _priority[s]) break;
        Swap(i, s);
        i = s;
      }
    }

    return root;
  }

  void Swap(int a, int b)
  {
    (_heap[a], _heap[b]) = (_heap[b], _heap[a]);
    (_priority[a], _priority[b]) = (_priority[b], _priority[a]);
  }
}