using System;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : IComparable<Vertex>
{
    public float X;
    public float Y;
    public float Z;
    public List<Vertex> Neighbours;

    public float Noise;
    public int Biome;

    public Vertex(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
        Neighbours = new List<Vertex>();
    }

    public int CompareTo(Vertex other)
    {
        if (other == null)
        {
            return 1;
        }

        if (X != other.X)
        {
            return X.CompareTo(other.X);
        }
        if (Y != other.Y)
        {
            return Y.CompareTo(other.Y);
        }
        return Z.CompareTo(other.Z);
    }

    public float Distance(Vertex other)
    {
        float dx = other.X - X;
        float dy = other.Y - Y;
        float dz = other.Z - Z;
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public bool OnSurface()
    {
        foreach (Vertex neighbour in Neighbours)
        {
            if (Biome != neighbour.Biome)
            {
                return false;
            }
        }
        return true;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }

    public static Vertex operator +(Vertex a, Vertex b) => new Vertex(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vertex operator -(Vertex a, Vertex b) => new Vertex(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vertex operator *(Vertex a, float b) => new Vertex(a.X * b, a.Y * b, a.Z * b);
    public static Vertex operator /(Vertex a, float b) => new Vertex(a.X / b, a.Y / b, a.Z / b);
}