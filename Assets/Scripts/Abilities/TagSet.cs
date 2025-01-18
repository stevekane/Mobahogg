using System;
using System.Collections.Generic;

namespace Abilities {
  [Serializable]
  public class TagSet{
    public static bool Overlap(TagSet ts1, TagSet ts2) {
      foreach (var t1 in ts1.Tags) {
        foreach (var t2 in ts2.Tags) {
          if (t1 == t2)
            return true;
        }
      }
      return false;
    }

    public static bool Contains(TagSet ts, Tag t) => ts.Tags.Contains(t);

    public List<Tag> Tags = new();
  }
}