public class Triangle
{
    public Vertex A;
    public Vertex B;
    public Vertex C;

    public Vertex[] Vertices;
    public bool IsValidForWall;

    public Triangle(Vertex a, Vertex b, Vertex c)
    {
        A = a;
        B = b;
        C = c;

        Vertices = new Vertex[] { A, B, C };
        IsValidForWall = true;
    }
}