using UnityEngine;

namespace ProjectS.Gameplay.Skills
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TempestEdgeConeMesh : MonoBehaviour
    {
        [SerializeField] private float radius = 6f;
        [SerializeField] private float angle = 120f;
        [SerializeField] private int segments = 24;

        private void Awake()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            filter.sharedMesh = BuildMesh();
        }

        private Mesh BuildMesh()
        {
            int clampedSegments = Mathf.Clamp(segments, 3, 128);
            float halfAngle = Mathf.Deg2Rad * angle * 0.5f;
            int vertexCount = clampedSegments + 2;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[clampedSegments * 3];

            vertices[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0f);

            for (int i = 0; i <= clampedSegments; i++)
            {
                float t = (float)i / clampedSegments;
                float angleRad = Mathf.Lerp(-halfAngle, halfAngle, t);
                float x = Mathf.Sin(angleRad) * radius;
                float z = Mathf.Cos(angleRad) * radius;
                vertices[i + 1] = new Vector3(x, 0f, z);
                uvs[i + 1] = new Vector2(t, 1f);
            }

            for (int i = 0; i < clampedSegments; i++)
            {
                int baseIndex = i * 3;
                triangles[baseIndex] = 0;
                triangles[baseIndex + 1] = i + 1;
                triangles[baseIndex + 2] = i + 2;
            }

            Mesh mesh = new Mesh
            {
                name = "TempestEdgeCone"
            };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
