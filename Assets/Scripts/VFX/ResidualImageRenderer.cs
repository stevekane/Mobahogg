using UnityEngine;

public class ResidualImageRenderer : MonoBehaviour {
  [SerializeField] SkinnedMeshRenderer[] SkinnedMeshRenderers;
  [SerializeField] string LayerName = "Visual";
  [SerializeField] Material Material;
  [Range(0,1)]
  public float Opacity = 1;
  public Color Color = Color.black;
  public float LifeTime = 1;

  public void RenderImage() {
    foreach (var skinnedMeshRenderer in SkinnedMeshRenderers) {
      var mesh = new Mesh();
      var image = new GameObject("Residual Image");
      var meshRenderer = image.AddComponent<MeshRenderer>();
      var meshFilter = image.AddComponent<MeshFilter>();
      skinnedMeshRenderer.BakeMesh(mesh);
      meshFilter.mesh = mesh;
      meshRenderer.material = Material;
      meshRenderer.material.SetColor("_Color", Color);
      meshRenderer.material.SetFloat("_Opacity", Opacity);
      meshRenderer.material.SetFloat("_StartTime", Time.time);
      meshRenderer.material.SetFloat("_EndTime", Time.time + LifeTime);
      image.layer = LayerMask.NameToLayer(LayerName);
      image.transform.SetPositionAndRotation(skinnedMeshRenderer.transform.position, skinnedMeshRenderer.transform.rotation);
      image.transform.localScale = skinnedMeshRenderer.transform.localScale;
      Destroy(image, LifeTime);
    }
  }
}