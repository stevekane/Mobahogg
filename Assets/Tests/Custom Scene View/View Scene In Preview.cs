using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Reflection;

/*
This is a cool exercise that has taught me what is required for setting up a preview scene.

In particular, I have learned how to flag a scene in a very weird way using reflection and
duck-typing to pass in internal PreviewSceneFlags.

HOWEVER, this has actually caused me to realize that the game object inside a preview scene
such as the scene created by PreviewRenderUtility already do not get these Monobehavior events (
Start, Update, FixedUpdate, etc)

Curiously, it seems that they MAY get calls like OnAnimatorIK and OnAnimatorMove ... a truly
strange fact but here we are.


SOME KEY LEARNINGS:

Preview Scenes should contain cameras that have an explicit "scene" set. That is completely
critical because it determines what objects are rendered by this camera. Additionally, preview
render utility seems to prefer to disable occlusion culling and it marks the cameratype as Preview.

To get proper rendering to happen with a full scriptable pipeline, you still want to use the
RenderPipeline.SendCameraRequest type of thing where you must have the request type setup
appropriately for the SRP that you are actually using.
*/

public static class PreviewSceneUtility {
    /// <summary>
    /// Creates a new preview scene using internal Unity APIs.
    /// </summary>
    /// <param name="allocateSceneCullingMask">Whether to allocate the scene culling mask.</param>
    /// <param name="customFlags">
    /// The integer value representing your custom flags. For instance, if you want:
    /// IsPreviewScene (1) | AllowCamerasForRendering (2) | AllowGlobalIlluminationLights (8)
    /// you would pass in 1 | 2 | 8 = 11.
    /// </param>
    /// <returns>The created preview scene.</returns>
    public static Scene CreateNewPreviewScene(bool allocateSceneCullingMask, int customFlags)
    {
        // Get the internal method "NewPreviewScene" from EditorSceneManager.
        MethodInfo newPreviewSceneMethod = typeof(EditorSceneManager)
            .GetMethod("NewPreviewScene", BindingFlags.NonPublic | BindingFlags.Static);

        if (newPreviewSceneMethod == null)
            throw new Exception("Could not find EditorSceneManager.NewPreviewScene method.");

        // Retrieve the internal type "UnityEditor.SceneManagement.PreviewSceneFlags"
        Type previewSceneFlagsType = typeof(EditorSceneManager).Assembly
            .GetType("UnityEditor.SceneManagement.PreviewSceneFlags");

        if (previewSceneFlagsType == null)
            throw new Exception("Could not find the PreviewSceneFlags type.");

        // Create an enum value from the integer customFlags.
        object previewSceneFlags = Enum.ToObject(previewSceneFlagsType, customFlags);

        // Call the internal method.
        object[] parameters = new object[] { allocateSceneCullingMask, previewSceneFlags };
        Scene previewScene = (Scene)newPreviewSceneMethod.Invoke(null, parameters);
        return previewScene;
    }
}

public class PreviewSceneTestMonobehavior : MonoBehaviour {
  void Start() {
    Debug.Log($"Start {name}");
  }
  void OnDestroy() {
    Debug.Log($"OnDestroy {name}");
  }
  void Update() {
    Debug.Log($"Update {name}");
  }
  void LateUpdate() {
    Debug.Log($"LateUpdate {name}");
  }
  void FixedUpdate() {
    Debug.Log($"FixedUpdate {name}");
  }
}

public class ViewSceneInPreview : EditorWindow
{
    // Define our own enum matching the internal PreviewSceneFlags so we can pass the proper flags.
    [Flags]
    private enum MyPreviewSceneFlags
    {
        NoFlags = 0,
        IsPreviewScene = 1,
        AllowCamerasForRendering = 2,
        AllowMonoBehaviourEvents = 4,
        AllowGlobalIlluminationLights = 8,
        AllowAutoPlayAudioSources = 0x10,
        AllFlags = 0x1F
    }

    // We'll keep references to our preview scene, camera, and render texture.
    private Scene previewScene;
    private Camera previewCamera;
    private RenderTexture previewTexture;

    [MenuItem("Window/View Scene In Preview")]
    public static void ShowWindow()
    {
        GetWindow<ViewSceneInPreview>("ViewSceneInPreview");
    }

    private void OnEnable()
    {
        MyPreviewSceneFlags myFlags =
          MyPreviewSceneFlags.IsPreviewScene |
          MyPreviewSceneFlags.AllowCamerasForRendering |
          MyPreviewSceneFlags.AllowGlobalIlluminationLights;
        previewScene = PreviewSceneUtility.CreateNewPreviewScene(true, (int)myFlags);

        GameObject cameraGO = new GameObject("PreviewCamera");
        previewCamera = cameraGO.AddComponent<Camera>();
        previewCamera.enabled = false;
        previewCamera.useOcclusionCulling = false;
        previewCamera.cameraType = CameraType.Preview;
        previewCamera.fieldOfView = 60f;
        previewCamera.nearClipPlane = 0.3f;
        previewCamera.farClipPlane = 1000f;
        // Extremely important! This instructs the camera to only render objects in this scene
        previewCamera.scene = previewScene;
        cameraGO.transform.position = new Vector3(2f, 2f, -5f);
        cameraGO.transform.LookAt(Vector3.zero);
        SceneManager.MoveGameObjectToScene(cameraGO, previewScene);
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = Vector3.zero;
        SceneManager.MoveGameObjectToScene(plane, previewScene);
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "My Preview Box";
        box.AddComponent<PreviewSceneTestMonobehavior>();
        box.transform.position = new Vector3(0f, 1f, 0f);
        SceneManager.MoveGameObjectToScene(box, previewScene);
    }

    private void OnDisable()
    {
        if (previewScene.IsValid())
        {
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
        if (previewTexture != null)
        {
            previewTexture.Release();
            previewTexture = null;
        }
    }

    private void OnGUI()
    {
        Rect rect = GUILayoutUtility.GetRect(position.width, position.height);
        int rtWidth = (int)rect.width;
        int rtHeight = (int)rect.height;
        if (previewTexture == null || previewTexture.width != rtWidth || previewTexture.height != rtHeight)
        {
            if (previewTexture != null)
            {
                previewTexture.Release();
            }
            previewTexture = new RenderTexture(rtWidth, rtHeight, 16, RenderTextureFormat.ARGB32);
            previewCamera.targetTexture = previewTexture;
        }
        previewCamera.Render();
        GUI.DrawTexture(rect, previewTexture, ScaleMode.StretchToFill, false);
        Repaint();
    }
}
