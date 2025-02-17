#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(PreviewLifecycle))]
public class PreviewLifeCycleTestEditor : Editor {
  public override VisualElement CreateInspectorGUI() {
    VisualElement root = new VisualElement();
    root.Add(new PreviewElement());
    return root;
  }
}

class PreviewElement : VisualElement {
  PreviewRenderUtility m_Preview;
  GameObject m_Cube;
  GameObject m_Ground;
  GameObject m_Light;
  Image m_Image;
  IVisualElementScheduledItem m_Scheduler;
  float m_StartTime;

  public PreviewElement() {
    style.width = 256;
    style.height = 256;
    m_Image = new Image();
    Add(m_Image);
    RegisterCallback<AttachToPanelEvent>(OnAttach);
    RegisterCallback<DetachFromPanelEvent>(OnDetach);
  }

  void OnAttach(AttachToPanelEvent evt) {
    m_StartTime = (float)EditorApplication.timeSinceStartup;
    m_Preview = new PreviewRenderUtility();
    m_Preview.cameraFieldOfView = 60f;
    m_Preview.camera.clearFlags = CameraClearFlags.SolidColor;
    m_Preview.camera.backgroundColor = Color.gray;
    m_Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    m_Cube.hideFlags = HideFlags.HideAndDontSave;
    m_Cube.transform.position = new Vector3(0,0.5f,0);
    m_Preview.AddSingleGO(m_Cube);
    m_Ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
    m_Ground.hideFlags = HideFlags.HideAndDontSave;
    m_Ground.transform.position = Vector3.zero;
    m_Preview.AddSingleGO(m_Ground);
    m_Light = new GameObject("Directional Light");
    m_Light.hideFlags = HideFlags.HideAndDontSave;
    Light lightComp = m_Light.AddComponent<Light>();
    lightComp.type = LightType.Directional;
    lightComp.intensity = 1.2f;
    m_Light.transform.rotation = Quaternion.Euler(50,-30,0);
    m_Preview.AddSingleGO(m_Light);
    m_Scheduler = schedule.Execute(UpdatePreview).Every(16);
  }

  void OnDetach(DetachFromPanelEvent evt) {
    m_Scheduler.Pause();
    m_Scheduler = null;
    m_Preview.Cleanup();
    GameObject.DestroyImmediate(m_Cube);
    GameObject.DestroyImmediate(m_Ground);
    GameObject.DestroyImmediate(m_Light);
  }

  void UpdatePreview() {
    if(m_Preview == null) return;
    float t = (float)EditorApplication.timeSinceStartup;
    float angle = (t - m_StartTime) * 180f;
    float rad = angle * Mathf.Deg2Rad;
    float r = Mathf.Sqrt(25f - 4f);
    Vector3 camPos = new Vector3(r * Mathf.Sin(rad),2f,r * Mathf.Cos(rad));
    m_Preview.camera.transform.position = camPos;
    m_Preview.camera.transform.LookAt(new Vector3(0,0.5f,0));
    Rect rct = contentRect;
    if (rct.width > 0 && rct.height > 0) {
      m_Preview.BeginPreview(rct,GUIStyle.none);
      m_Preview.camera.Render();
      Texture tex = m_Preview.EndPreview();
      m_Image.image = tex;
    }
  }
}
#endif
