using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EthicalLab.Presentation
{
    /// <summary>Biseles pequeños en mobiliario. Mantiene escala, anclas y colliders originales.</summary>
    public static class OfficeBevel
    {
        static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        public static void Apply(GameObject go, Vector3 size)
        {
            float smallest = Mathf.Min(size.x, Mathf.Min(size.y, size.z));
            if (smallest < 0.045f || Mathf.Max(size.x, Mathf.Max(size.y, size.z)) > 3.5f) return;
            string key = string.Format(CultureInfo.InvariantCulture, "Bevel_{0:F4}_{1:F4}_{2:F4}", size.x, size.y, size.z);
            if (!meshes.TryGetValue(key, out var mesh) || mesh == null)
            {
                mesh = Create(size, Mathf.Min(0.012f, smallest * 0.16f));
                mesh.name = key;
                meshes[key] = mesh;
            }
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        static Mesh Create(Vector3 size, float radius)
        {
            var vertices = new List<Vector3>(96);
            var normals = new List<Vector3>(96);
            var uv = new List<Vector2>(96);
            var triangles = new List<int>(324);
            var half = size * 0.5f;
            var core = half - Vector3.one * radius;
            Vector3[] ns = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (var n in ns)
            {
                Vector3 u = Mathf.Abs(n.y) > 0.5f ? Vector3.right : Vector3.up;
                Vector3 v = Vector3.Cross(n, u);
                float hu = Vector3.Dot(half, new Vector3(Mathf.Abs(u.x), Mathf.Abs(u.y), Mathf.Abs(u.z)));
                float hv = Vector3.Dot(half, new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z)));
                float hn = Vector3.Dot(half, new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z)));
                float[] us = { -hu, -hu + radius, hu - radius, hu };
                float[] vs = { -hv, -hv + radius, hv - radius, hv };
                int start = vertices.Count;
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 4; x++)
                    {
                        Vector3 p = n * hn + u * us[x] + v * vs[y];
                        Vector3 c = new Vector3(Mathf.Clamp(p.x, -core.x, core.x), Mathf.Clamp(p.y, -core.y, core.y), Mathf.Clamp(p.z, -core.z, core.z));
                        Vector3 normal = (p - c).normalized;
                        p = c + normal * radius;
                        vertices.Add(new Vector3(p.x / size.x, p.y / size.y, p.z / size.z));
                        normals.Add(Vector3.Scale(normal, size).normalized);
                        uv.Add(new Vector2((us[x] + hu) / (2 * hu), (vs[y] + hv) / (2 * hv)));
                    }
                for (int y = 0; y < 3; y++)
                    for (int x = 0; x < 3; x++)
                    {
                        int a = start + y * 4 + x;
                        triangles.Add(a); triangles.Add(a + 1); triangles.Add(a + 4);
                        triangles.Add(a + 1); triangles.Add(a + 5); triangles.Add(a + 4);
                    }
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
