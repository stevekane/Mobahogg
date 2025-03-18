using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ViewSceneInPreview : EditorWindow {
  Scene scene;
  PhysicsScene physicsScene;
  Camera camera;
  RenderTexture renderTexture;
  double previousTime;
  double currentTime;
  double timer;
  double dt;
  GameObject box;  // Reference to our box

  [MenuItem("Window/View Scene In Preview")]
  public static void ShowWindow() {
    GetWindow<ViewSceneInPreview>("ViewSceneInPreview");
  }

  void OnEnable() {
    previousTime = EditorApplication.timeSinceStartup;
    currentTime = EditorApplication.timeSinceStartup;
    dt = 0;
    timer = 0;

    scene = EditorSceneManager.NewPreviewScene();
    scene.name = "Preview Scene";
    physicsScene = scene.GetPhysicsScene();

    GameObject cameraGO = new GameObject("PreviewCamera");
    camera = cameraGO.AddComponent<Camera>();
    camera.enabled = false;
    camera.cameraType = CameraType.Preview;
    camera.useOcclusionCulling = false;
    camera.fieldOfView = 60f;
    camera.nearClipPlane = 0.3f;
    camera.farClipPlane = 1000f;
    // Instruct the camera to only render objects in this scene
    camera.scene = scene;
    cameraGO.transform.position = new Vector3(2f, 2f, -5f);
    cameraGO.hideFlags = HideFlags.HideAndDontSave;
    cameraGO.transform.LookAt(Vector3.zero);

    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
    plane.transform.localScale = new Vector3(10, 10, 10);
    plane.isStatic = true;

    // Create the box and keep a reference for resetting
    box = GameObject.CreatePrimitive(PrimitiveType.Cube);
    box.AddComponent<Rigidbody>();
    box.transform.position = new Vector3(0f, 10f, 0f);

    EditorSceneManager.MoveGameObjectToScene(cameraGO, scene);
    EditorSceneManager.MoveGameObjectToScene(plane, scene);
    EditorSceneManager.MoveGameObjectToScene(box, scene);
  }

  void OnDisable() {
    if (scene.IsValid()) {
      EditorSceneManager.ClosePreviewScene(scene);
    }
    if (renderTexture != null) {
      renderTexture.Release();
      renderTexture = null;
    }
  }

  void OnGUI() {
    Rect rect = GUILayoutUtility.GetRect(position.width, position.height);
    int rtWidth = (int)rect.width;
    int rtHeight = (int)rect.height;
    if (renderTexture == null || renderTexture.width != rtWidth || renderTexture.height != rtHeight) {
      if (renderTexture != null) {
        renderTexture.Release();
      }
      renderTexture = new RenderTexture(rtWidth, rtHeight, 16, RenderTextureFormat.ARGB32);
      camera.targetTexture = renderTexture;
    }

    // Handle mouse click: on click, reset the box position.
    if (Event.current.type == EventType.MouseDown) {
      Event.current.Use();
      box.GetComponent<Rigidbody>().position = new(0,10,0);
      box.GetComponent<Rigidbody>().rotation = Quaternion.identity;
      box.GetComponent<Rigidbody>().PublishTransform();
    }

    SimulatePhysics();
    camera.Render();
    GUI.DrawTexture(rect, renderTexture, ScaleMode.StretchToFill, false);
    Repaint();
  }

  void SimulatePhysics() {
    previousTime = currentTime;
    currentTime = EditorApplication.timeSinceStartup;
    dt = currentTime - previousTime;
    timer += dt;
    while (timer >= Time.fixedDeltaTime) {
      timer -= Time.fixedDeltaTime;
      physicsScene.Simulate(Time.fixedDeltaTime);
    }
  }
}
