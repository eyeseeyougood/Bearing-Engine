using OpenTK.Mathematics;

namespace Bearing;

public class Mesh
{
    public MeshVertex3D[] vertices;
    public uint[] indices;

    public virtual float[] GetVertexData()
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
