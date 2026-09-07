using System.Collections.Generic;
using UnityEngine;

public static class GeoJsonPolygonMeshBuilder
{
    public static Mesh CreateFanMesh(List<Vector3> vertices, string meshName)
    {
        Vector3 center = Vector3.zero;

        foreach (Vector3 vertex in vertices)
            center += vertex;

        center /= vertices.Count;

        List<Vector3> meshVertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        meshVertices.Add(center);

        for (int i = 0; i < vertices.Count; i++)
            meshVertices.Add(vertices[i]);

        for (int i = 1; i < meshVertices.Count; i++)
        {
            int next = i + 1;

            if (next >= meshVertices.Count)
                next = 1;

            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(next);

            triangles.Add(0);
            triangles.Add(next);
            triangles.Add(i);
        }

        Mesh mesh = new Mesh();
        mesh.name = meshName;
        mesh.SetVertices(meshVertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        return mesh;
    }
}