using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "A Fucking List", menuName = "Frame Behaviors/A Fucking List")]
public class AFuckingList : ScriptableObject {
  [SerializeReference]
  public List<AFuckingItem> Items = new();
}

[Serializable]
public abstract class AFuckingItem {
  public string Name;
}

[Serializable]
public class StringItem : AFuckingItem {
  public string MyString;
}

[Serializable]
public class IntItem : AFuckingItem {
  public int MyInt;
}