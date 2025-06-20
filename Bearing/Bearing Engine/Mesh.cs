using OpenTK.Mathematics;

namespace Bearing;



public abstract class Mesh
{
    public abstract float[] GetVertexData();
    public uint[] indices = new uint[0];
}




public class Mesh3D : Mesh
{
    public MeshVertex3D[] vertices;

    public Mesh3D(string filename, bool engineResource = false)
    {
        string res = engineResource ? $"./EngineData/Models/{filename}" : $"./Resources/Models/{filename}";
        Mesh3D m = ModelLoader.FileToMesh3D(res);
        vertices = m.vertices;
        indices = m.indices;
    }

    private Mesh3D() { }

    public static Mesh3D CreateEmpty() { return new Mesh3D(); }
    public static Mesh3D FromData(MeshVertex3D[] verts, uint[] indices)
    {
        return new Mesh3D() {
            vertices = verts,
            indices = indices
        };
    }

    public override float[] GetVertexData()
    {
        List<float> result = new();

        for (int i = 0; i < vertices.Length; i++)
        {
            result.AddRange(vertices[i].GetData());
        }

        return result.ToArray();
    }

    public Vector3 GetBoundingBox()
    {
        if (vertices == null || vertices.Length == 0)
            return Vector3.Zero;

        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (MeshVertex3D vertex in vertices)
        {
            Vector3 pos = vertex.position;
            min.X = Math.Min(min.X, pos.X);
            min.Y = Math.Min(min.Y, pos.Y);
            min.Z = Math.Min(min.Z, pos.Z);

            max.X = Math.Max(max.X, pos.X);
            max.Y = Math.Max(max.Y, pos.Y);
            max.Z = Math.Max(max.Z, pos.Z);
        }

        return max - min;
    }
}







public class Mesh2D : Mesh
{
    public MeshVertex2D[] vertices;

    public Mesh2D(string filename, bool engineResource = false)
    {
        string res = engineResource ? $"./EngineData/Models/{filename}" : $"./Resources/Models/{filename}";
        Mesh2D m = ModelLoader.FileToMesh2D(res);
        vertices = m.vertices;
        indices = m.indices;
    }

    private Mesh2D() { }

    public static Mesh2D CreateEmpty() { return new Mesh2D(); }
    public static Mesh2D FromData(MeshVertex2D[] verts, uint[] indices)
    {
        return new Mesh2D()
        {
            vertices = verts,
            indices = indices
        };
    }

    public override float[] GetVertexData()
    {
        List<float> result = new();

        for (int i = 0; i < vertices.Length; i++)
        {
            result.AddRange(vertices[i].GetData());
        }

        return result.ToArray();
    }

    public Vector3 GetBoundingBox()
    {
        if (vertices == null || vertices.Length == 0)
            return Vector3.Zero;

        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (MeshVertex2D vertex in vertices)
        {
            Vector2 pos = vertex.position;
            min.X = Math.Min(min.X, pos.X);
            min.Y = Math.Min(min.Y, pos.Y);

            max.X = Math.Max(max.X, pos.X);
            max.Y = Math.Max(max.Y, pos.Y);
        }

        return max - min;
    }
}
