using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGenerator
{
    private const int PoissonPointLimit = 200000;

    private int _seed;

    private float _pointRadius;
    private Vector2 _sampleRegion;

    private NoiseSettings _noiseSettings;
    private float _terraceHeight;
    private Vector2 _fallOffRegion;
    private int _extensionStepCount;
    private float _detailOffset;

    private Biome[] _biomes;

    public TerrainGenerator(TerrainSettings terrainSettings)
    {
        _seed = terrainSettings.Seed;

        _pointRadius = terrainSettings.PointRadius;
        _sampleRegion = terrainSettings.SampleRegion;

        _noiseSettings = terrainSettings.NoiseSettings;
        _terraceHeight = terrainSettings.TerraceHeight;
        _fallOffRegion = terrainSettings.FallOffRegion;
        _extensionStepCount = terrainSettings.ExtensionStepCount;
        _detailOffset = terrainSettings.DetailOffset;

        _biomes = terrainSettings.Biomes;
    }

    public Mesh GenerateMesh(TerrainData terrainData)
    {
        MeshBuilder meshBuilder = BuildMesh(terrainData);
        if (_detailOffset > 0)
        {
            DetailTerrain(meshBuilder, _detailOffset);
        }
        return meshBuilder.CreateMesh();
    }

    public TerrainData GenerateTerrainData()
    {
        Vector2[] points = GeneratePoints();
        List<Triangle> triangles = DelaunayTriangulation.GenerateTriangles(points);

        TerrainData terrainData = new TerrainData();
        HashSet<Vertex> vertexSet = new HashSet<Vertex>();
        foreach (Triangle triangle in triangles)
        {
            for (int i = 0; i < triangle.Vertices.Length; i++)
            {
                Vertex vertex = triangle.Vertices[i];
                if (!vertexSet.Contains(vertex))
                {
                    vertexSet.Add(vertex);
                    terrainData.Vertices.Add(vertex);
                }
                for (int j = 0; j < 2; j++)
                {
                    Vertex neighbour = triangle.Vertices[(i + 1 + j) % 3];
                    if (!vertex.Neighbours.Contains(neighbour))
                    {
                        vertex.Neighbours.Add(neighbour);
                    }
                }
            }
            terrainData.Triangles.Add(triangle);
        }

        Vector2[] noisePoints = new Vector2[terrainData.Vertices.Count];
        for (int i = 0; i < noisePoints.Length; i++)
        {
            noisePoints[i] = new Vector2(terrainData.Vertices[i].X, terrainData.Vertices[i].Z);
        }
        Noise noise = new Noise(_noiseSettings, _seed);
        float[] heights = noise.GetHeights(noisePoints);
        noise.NormalizeHeights(heights);
        noise.AddFallOff(noisePoints, heights, _sampleRegion, _fallOffRegion);

        for (int i = 0; i < terrainData.Vertices.Count; i++)
        {
            Vertex vertex = terrainData.Vertices[i];
            vertex.Noise = heights[i];
            for (int j = 0; j < _biomes.Length; j++)
            {
                if (heights[i] <= _biomes[j].Height)
                {
                    vertex.Biome = j;
                    break;
                }
            }
            vertex.Y = vertex.Biome * _terraceHeight;
        }
        LevelTriangles(terrainData);
        return terrainData;
    }

    private void LevelTriangles(TerrainData terrainData)
    {
        for (int i = 0; i < _biomes.Length - 2; i++)
        {
            bool validTerrain = true;
            foreach (Triangle triangle in terrainData.Triangles)
            {
                int minBiome = triangle.Vertices.Min(v => v.Biome);
                int maxBiome = triangle.Vertices.Max(v => v.Biome);
                if (maxBiome - minBiome >= 2)
                {
                    foreach (Vertex vertex in triangle.Vertices.Where(v => v.Biome != minBiome))
                    {
                        vertex.Biome = minBiome + 1;
                        vertex.Y = vertex.Biome * _terraceHeight;

                        float minNoise = _biomes[vertex.Biome - 1].Height;
                        float maxNoise = _biomes[vertex.Biome].Height;
                        float averageNoise = vertex.Neighbours.Average(v => v.Noise);
                        vertex.Noise = Mathf.Clamp(averageNoise, minNoise, maxNoise);
                    }
                    validTerrain = false;
                }
            }
            if (validTerrain)
            {
                break;
            }
        }
    }

    private MeshBuilder BuildMesh(TerrainData terrainData)
    {
        MarkInvalidTriangles(terrainData);
        MeshBuilder meshBuilder = new MeshBuilder();
        foreach (Triangle triangle in terrainData.Triangles)
        {
            Vertex a = triangle.A;
            Vertex b = triangle.B;
            Vertex c = triangle.C;

            Vector3 v0 = a.ToVector3();
            Vector3 v1 = b.ToVector3();
            Vector3 v2 = c.ToVector3();

            if (a.Biome != b.Biome || b.Biome != c.Biome || c.Biome != a.Biome)
            {
                bool v0Up = b.Biome == c.Biome && b.Biome < a.Biome;
                bool v1Up = c.Biome == a.Biome && c.Biome < b.Biome;
                bool v2Up = a.Biome == b.Biome && a.Biome < c.Biome;
                bool v0Down = b.Biome == c.Biome && b.Biome > a.Biome;
                bool v1Down = c.Biome == a.Biome && c.Biome > b.Biome;
                bool v2Down = a.Biome == b.Biome && a.Biome > c.Biome;

                if (v0Up || v1Up || v2Up)
                {
                    if (v0Up)
                    {
                        meshBuilder.AddTriangle(new Vector3(v0.x, v1.y, v0.z), v1, v2, GetColor(b.Noise, _biomes));
                    }
                    else if (v1Up)
                    {
                        meshBuilder.AddTriangle(v0, new Vector3(v1.x, v2.y, v1.z), v2, GetColor(c.Noise, _biomes));
                    }
                    else if (v2Up)
                    {
                        meshBuilder.AddTriangle(v0, v1, new Vector3(v2.x, v0.y, v2.z), GetColor(a.Noise, _biomes));
                    }
                }
                else if (v0Down || v1Down || v2Down)
                {
                    if (v0Down)
                    {
                        if (triangle.IsValidForWall)
                        {
                            meshBuilder.AddTriangle(new Vector3(v1.x, v0.y, v1.z), v1, v2, GetColor(b.Noise, _biomes, true));
                            meshBuilder.AddTriangle(v2, new Vector3(v2.x, v0.y, v2.z), new Vector3(v1.x, v0.y, v1.z), GetColor(b.Noise, _biomes, true));
                        }
                        meshBuilder.AddTriangle(v0, new Vector3(v1.x, v0.y, v1.z), new Vector3(v2.x, v0.y, v2.z), GetColor(a.Noise, _biomes));
                    }
                    else if (v1Down)
                    {
                        if (triangle.IsValidForWall)
                        {
                            meshBuilder.AddTriangle(v0, new Vector3(v0.x, v1.y, v0.z), new Vector3(v2.x, v1.y, v2.z), GetColor(c.Noise, _biomes, true));
                            meshBuilder.AddTriangle(new Vector3(v2.x, v1.y, v2.z), v2, v0, GetColor(c.Noise, _biomes, true));
                        }
                        meshBuilder.AddTriangle(new Vector3(v0.x, v1.y, v0.z), v1, new Vector3(v2.x, v1.y, v2.z), GetColor(b.Noise, _biomes));
                    }
                    else if (v2Down)
                    {
                        if (triangle.IsValidForWall)
                        {
                            meshBuilder.AddTriangle(v0, v1, new Vector3(v1.x, v2.y, v1.z), GetColor(a.Noise, _biomes, true));
                            meshBuilder.AddTriangle(new Vector3(v1.x, v2.y, v1.z), new Vector3(v0.x, v2.y, v0.z), v0, GetColor(a.Noise, _biomes, true));
                        }
                        meshBuilder.AddTriangle(new Vector3(v0.x, v2.y, v0.z), new Vector3(v1.x, v2.y, v1.z), v2, GetColor(c.Noise, _biomes));
                    }
                }
                else
                {
                    Debug.LogError("Invalid Triangle resulting in hole");
                }
            }
            else
            {
                float noise = (a.Noise + b.Noise + c.Noise) / 3;
                meshBuilder.AddTriangle(v0, v1, v2, GetColor(noise, _biomes));
            }
        }
        return meshBuilder;
    }

    private void MarkInvalidTriangles(TerrainData terrainData)
    {
        Dictionary<(Vertex, Vertex), Triangle> vertexPairSet = new Dictionary<(Vertex, Vertex), Triangle>();
        (Vertex, Vertex) GetVertexPair(Vertex a, Vertex b)
        {
            return a.CompareTo(b) < 0 ? (a, b) : (b, a);
        }

        foreach (Triangle triangle in terrainData.Triangles)
        {
            Vertex a = triangle.A;
            Vertex b = triangle.B;
            Vertex c = triangle.C;

            if (a.Biome != b.Biome || b.Biome != c.Biome || c.Biome != a.Biome)
            {
                bool aDown = b.Biome == c.Biome && b.Biome > a.Biome;
                bool bDown = c.Biome == a.Biome && c.Biome > b.Biome;
                bool cDown = a.Biome == b.Biome && a.Biome > c.Biome;

                if (aDown || bDown || cDown)
                {
                    (Vertex, Vertex) vertexPair;
                    if (aDown)
                    {
                        vertexPair = GetVertexPair(b, c);
                    }
                    else if (bDown)
                    {
                        vertexPair = GetVertexPair(c, a);
                    }
                    else
                    {
                        vertexPair = GetVertexPair(a, b);
                    }

                    if (!vertexPairSet.ContainsKey(vertexPair))
                    {
                        vertexPairSet.Add(vertexPair, triangle);
                    }
                    else if (triangle.IsValidForWall && vertexPairSet[vertexPair].IsValidForWall)
                    {
                        triangle.IsValidForWall = false;
                        vertexPairSet[vertexPair].IsValidForWall = false;
                    }
                    else
                    {
                        Debug.LogError("Invalid Triangle already marked");
                    }
                }
            }
        }
    }

    private Vector2[] GeneratePoints()
    {
        Vector2[] poissonPoints = PoissonDiscSampling.GeneratePoints(_pointRadius, _sampleRegion, PoissonPointLimit);
        for (int i = 0; i < poissonPoints.Length; i++)
        {
            poissonPoints[i] -= _sampleRegion / 2;
        }
        List<Vector2> points = new List<Vector2>(poissonPoints);

        float rectGrow = _pointRadius;
        for (int i = 0; i < _extensionStepCount; i++)
        {
            AddRectanglePoints(points, _sampleRegion + Vector2.one * rectGrow, _pointRadius * (i + 2));
            rectGrow += _pointRadius * (i + 1) * 4;
        }

        return points.ToArray();
    }

    private void AddRectanglePoints(List<Vector2> points, Vector2 size, float distance)
    {
        Vector2Int steps = new Vector2Int((int)(size.x / distance), (int)(size.y / distance));
        Vector2 botLeft = -size / 2;
        Vector2 topRight = size / 2;
        Vector2 botRight = new Vector2(topRight.x, botLeft.y);
        Vector2 topLeft = new Vector2(botLeft.x, topRight.y);

        for (int i = 0; i <= steps.x; i++)
        {
            float lerpTime = (float)i / steps.x;
            points.Add(Vector2.Lerp(botLeft, botRight, lerpTime));
            points.Add(Vector2.Lerp(topLeft, topRight, lerpTime));
        }

        for (int i = 1; i <= steps.y - 1; i++)
        {
            float lerpTime = (float)i / steps.y;
            points.Add(Vector2.Lerp(botLeft, topLeft, lerpTime));
            points.Add(Vector2.Lerp(botRight, topRight, lerpTime));
        }
    }

    private Color GetColor(float noise, Biome[] biomes, bool isEdge = false)
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            if (noise <= biomes[i].Height)
            {
                if (isEdge)
                {
                    return biomes[i].Edge;
                }
                float colorValue = Mathf.InverseLerp(i == 0 ? 0 : biomes[i - 1].Height, biomes[i].Height, noise);
                return biomes[i].Gradient.Evaluate(colorValue);
            }
        }
        return Color.magenta;
    }

    private void DetailTerrain(MeshBuilder meshBuilder, float offset)
    {
        Dictionary<Vector3, float> vertexOffsets = new Dictionary<Vector3, float>();
        for (int i = 0; i < meshBuilder.Vertices.Count; i++)
        {
            Vector3 vertex = meshBuilder.Vertices[i];
            if (!vertexOffsets.ContainsKey(vertex))
            {
                float offsetY = Random.Range(-offset, offset);
                vertexOffsets.Add(vertex, offsetY);
                meshBuilder.Vertices[i] += Vector3.up * offsetY;
            }
            else
            {
                meshBuilder.Vertices[i] += Vector3.up * vertexOffsets[vertex];
            }
        }
    }
}