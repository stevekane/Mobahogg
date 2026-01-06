using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Scenes/Scene Reference")]
public class SceneReference : ScriptableObject, ISceneReference
{
#if UNITY_EDITOR
  [SerializeField] private SceneAsset sceneAsset;

  void OnValidate()
  {
    if (sceneAsset != null)
      scenePath = AssetDatabase.GetAssetPath(sceneAsset);
  }

#endif

  [SerializeField, HideInInspector]
  string scenePath;
  public string ScenePath => scenePath;
  public string SceneName => System.IO.Path.GetFileNameWithoutExtension(scenePath);
}