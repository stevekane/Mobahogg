using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "A Fucking List", menuName = "Frame Behaviors/A Fucking List")]
public class AFuckingList : ScriptableObject {
  public List<AFuckingItem> Items = new();
}

[Serializable]
public class AFuckingItem {
  public string Name;
}