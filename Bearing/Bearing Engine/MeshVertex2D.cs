using OpenTK.Mathematics;

namespace Bearing;

public struct MeshVertex2D
{
    public Vector2 position;
    public Vector2 texCoord;

    public static int dataSize = 4;
    public static int sizeInBytes = sizeof(float) * dataSize;

    public float[] GetData()
    {
        List<float> result = new List<float>();

        result.Add(position.X);
        result.Add(position.Y);
        result.Add(texCoord.X);
        result.Add(texCoord.Y);

        return result.ToArray();
    }
}