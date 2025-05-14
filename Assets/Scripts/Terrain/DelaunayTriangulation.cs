using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

using TDNVertex = TriangleNet.Geometry.Vertex;
using TDNTriangle = TriangleNet.Topology.Triangle;

public static class DelaunayTriangulation
{
    public static List<Triangle> GenerateTriangles(Vector2[] points)
    {
        List<TDNTriangle> tdnTriangles = GenerateTDNTriangles(points);
        List<Triangle> triangles = new List<Triangle>();

        Dictionary<TDNVertex, Vertex> vertexMap = new Dictionary<TDNVertex, Vertex>();
        foreach (TDNTriangle tdnTriangle in tdnTriangles)
        {
            Vertex a = new Vertex((float)tdnTriangle.vertices[0].x, 0, (float)tdnTriangle.vertices[0].y);
            Vertex b = new Vertex((float)tdnTriangle.vertices[1].x, 0, (float)tdnTriangle.vertices[1].y);
            Vertex c = new Vertex((float)tdnTriangle.vertices[2].x, 0, (float)tdnTriangle.vertices[2].y);
            if (!vertexMap.ContainsKey(tdnTriangle.vertices[0]))
            {
                vertexMap.Add(tdnTriangle.vertices[0], a);
            }
            if (!vertexMap.ContainsKey(tdnTriangle.vertices[1]))
            {
                vertexMap.Add(tdnTriangle.vertices[1], b);
            }
            if (!vertexMap.ContainsKey(tdnTriangle.vertices[2]))
            {
                vertexMap.Add(tdnTriangle.vertices[2], c);
            }
            Triangle triangle = new Triangle(vertexMap[tdnTriangle.vertices[2]], vertexMap[tdnTriangle.vertices[1]], vertexMap[tdnTriangle.vertices[0]]);
            triangles.Add(triangle);
        }
        return triangles;
    }

    private static List<TDNTriangle> GenerateTDNTriangles(Vector2[] points)
    {
        Polygon polygon = new Polygon();
        for (int i = 0; i < points.Length; i++)
        {
            TDNVertex vertex = new TDNVertex(points[i].x, points[i].y);
            vertex.original = true;
            polygon.Add(vertex);
        }
        ConstraintOptions constraints = new ConstraintOptions
        {
            ConformingDelaunay = true
        };
        TriangleNet.Mesh triangleMesh = polygon.Triangulate(constraints) as TriangleNet.Mesh;

        List<TDNTriangle> triangles = new List<TDNTriangle>();
        foreach (TDNTriangle triangle in triangleMesh.Triangles)
        {
            if (triangle.vertices.All(v => v.original))
            {
                triangles.Add(triangle);
            }
        }
        return triangles;
    }
}