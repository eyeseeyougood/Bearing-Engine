using OpenTK.Mathematics;

namespace Bearing;

public struct MeshVertex3D
{
    public Vector3 position;
    public Vector2 texCoord;
    public Vector3 normal;

    public static int dataSize = 8;
    public static int sizeInBytes = sizeof(float) * dataSize;

    public float[] GetData()
    {
        List<float> result = new List<float>();

        result.Add(position.X);
        result.Add(position.Y);
        result.Add(position.Z);
        result.Add(texCoord.X);
        result.Add(texCoord.Y);
        result.Add(normal.X);
        result.Add(normal.Y);
        result.Add(normal.Z);

        return result.ToArray();
    }
}