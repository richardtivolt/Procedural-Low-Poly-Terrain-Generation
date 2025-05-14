using System.Collections.Generic;

public class TerrainData
{
    private List<Vertex> _vertices;
    private List<Triangle> _triangles;

    public List<Vertex> Vertices => _vertices;
    public List<Triangle> Triangles => _triangles;

    public TerrainData()
    {
        _vertices = new List<Vertex>();
        _triangles = new List<Triangle>();
    }
}