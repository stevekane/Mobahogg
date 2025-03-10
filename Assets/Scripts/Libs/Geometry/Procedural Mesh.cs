using UnityEngine;

public static class ProceduralMesh {
  public static Mesh AnnularSector(float angle, float innerRadius, float outerRadius, int segments) {
    Mesh mesh = new Mesh();
    float segmentAngle = angle / segments;
    int vertexCount = (segments + 1) * 2;
    Vector3[] vertices = new Vector3[vertexCount];
    int[] triangles = new int[segments * 6];

    for (int i = 0; i <= segments; i++) {
      float currentAngleRad = Mathf.Deg2Rad * (-angle / 2f + i * segmentAngle);
      float cos = Mathf.Cos(currentAngleRad);
      float sin = Mathf.Sin(currentAngleRad);
      vertices[i * 2] = new Vector3(sin * outerRadius, 0f, cos * outerRadius);
      vertices[i * 2 + 1] = new Vector3(sin * innerRadius, 0f, cos * innerRadius);
    }

    for (int i = 0; i < segments; i++) {
      int startIndex = i * 2;
      triangles[i * 6] = startIndex;
      triangles[i * 6 + 1] = startIndex + 2;
      triangles[i * 6 + 2] = startIndex + 1;
      triangles[i * 6 + 3] = startIndex + 2;
      triangles[i * 6 + 4] = startIndex + 3;
      triangles[i * 6 + 5] = startIndex + 1;
    }
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    return mesh;
  }
}