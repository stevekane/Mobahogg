using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LookAtCamera : MonoBehaviour {
  void LateUpdate() {
    #if UNITY_EDITOR
    var camera = EditorApplication.isPlaying ? CameraManager.Instance.Active : Camera.main;
    #else
    var camera = CameraManager.Instance.Active;
    #endif
    if (camera)
      transform.LookAt(transform.position + camera.transform.forward);
  }
}