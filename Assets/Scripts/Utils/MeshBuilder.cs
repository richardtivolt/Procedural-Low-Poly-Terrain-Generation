using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector2> _uvs;
    private List<Color> _colors;

    public List<Vector3> Vertices => _vertices;
    public List<int> Triangles => _triangles;
    public List<Vector2> UVs => _uvs;
    public List<Color> Colors => _colors;

    public MeshBuilder()
    {
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _uvs = new List<Vector2>();
        _colors = new List<Color>();
    }

    public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color)
    {
        _vertices.Add(v0);
        _vertices.Add(v1);
        _vertices.Add(v2);

        int triangles = _triangles.Count;
        _triangles.Add(triangles);
        _triangles.Add(triangles + 1);
        _triangles.Add(triangles + 2);

        _uvs.Add(v0);
        _uvs.Add(v1);
        _uvs.Add(v2);

        _colors.Add(color);
        _colors.Add(color);
        _colors.Add(color);
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();
        mesh.uv = _uvs.ToArray();
        mesh.colors = _colors.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public void SimplifyMeshData(bool smooth)
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector2> newUVs = new List<Vector2>();
        List<Color> newColors = new List<Color>();

        Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();
        for (int i = 0; i < _triangles.Count; i++)
        {
            int originalIndex = _triangles[i];
            Vector3 originalVertex = _vertices[originalIndex];
            Vector2 originalUV = _uvs[originalIndex];
            Color originalColor = _colors[originalIndex];

            if (!vertexMap.ContainsKey(originalVertex))
            {
                vertexMap[originalVertex] = newVertices.Count;
                newVertices.Add(originalVertex);
                newUVs.Add(originalUV);
                newColors.Add(originalColor);
            }
            newTriangles.Add(vertexMap[originalVertex]);
        }

        if (smooth)
        {
            _vertices = newVertices;
            _triangles = newTriangles;
            _uvs = newUVs;
            _colors = newColors;
        }
        else
        {
            _vertices = new List<Vector3>();
            _triangles = new List<int>();
            _uvs = new List<Vector2>();
            _colors = new List<Color>();

            for (int i = 0; i < newTriangles.Count; i += 3)
            {
                int i1 = newTriangles[i];
                int i2 = newTriangles[i + 1];
                int i3 = newTriangles[i + 2];

                _vertices.Add(newVertices[i1]);
                _vertices.Add(newVertices[i2]);
                _vertices.Add(newVertices[i3]);

                _triangles.Add(i);
                _triangles.Add(i + 1);
                _triangles.Add(i + 2);

                _uvs.Add(newUVs[i1]);
                _uvs.Add(newUVs[i2]);
                _uvs.Add(newUVs[i3]);

                _colors.Add(newColors[i1]);
                _colors.Add(newColors[i2]);
                _colors.Add(newColors[i3]);
            }
        }
    }
}