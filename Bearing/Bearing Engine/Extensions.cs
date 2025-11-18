using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTK.Mathematics;

namespace Bearing;

public static class Extensions
{
    public static BulletSharp.Math.Matrix ToBulletMatrix(this Matrix4 mat)
    {
        return new BulletSharp.Math.Matrix(
            mat.M11,
            mat.M12,
            mat.M13,
            mat.M14,

            mat.M21,
            mat.M22,
            mat.M23,
            mat.M24,

            mat.M31,
            mat.M32,
            mat.M33,
            mat.M34,

            mat.M41,
            mat.M42,
            mat.M43,
            mat.M44
            );
    }

    public static Matrix4 ToTKMatrix(this BulletSharp.Math.Matrix mat)
    {
        return new Matrix4(
            mat.M11,
            mat.M12,
            mat.M13,
            mat.M14,

            mat.M21,
            mat.M22,
            mat.M23,
            mat.M24,

            mat.M31,
            mat.M32,
            mat.M33,
            mat.M34,

            mat.M41,
            mat.M42,
            mat.M43,
            mat.M44
            );
    }

    public static BulletSharp.Math.Vector3 ToBulletVector(this Vector3 vector)
    {
        return new BulletSharp.Math.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static Vector3 ToTKVector(this BulletSharp.Math.Vector3 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }

    public static float LerpAngle(this float a, float b, float t)
    {
        float delta = Repeat((b - a) + 180f, 360f) - 180f;
        return a + delta * t;
    }

    public static Vector3 ToEulerAngles(this Silk.NET.Maths.Quaternion<float> q)
    {
        float sinp = 2f * (q.W * q.X - q.Z * q.Y);

        float pitch;
        if (MathF.Abs(sinp) >= 1f)
            pitch = MathF.CopySign(MathF.PI / 2f, sinp);
        else
            pitch = MathF.Asin(sinp);

        float yaw = MathF.Atan2(
            2f * (q.W * q.Y + q.X * q.Z),
            1f - 2f * (q.Y * q.Y + q.X * q.X)
        );

        float roll = MathF.Atan2(
            2f * (q.W * q.Z + q.Y * q.X),
            1f - 2f * (q.Z * q.Z + q.X * q.X)
        );

        return new Vector3(pitch, yaw, roll);
    }

    private static float Repeat(float t, float length)
    {
        return t - MathF.Floor(t / length) * length;
    }


    public static SKColor ToSKColour(this BearingColour c)
    {
        Vector4 i = (Vector4)c.GetZeroTo255A();
        return new SKColor((byte)i.X, (byte)i.Y, (byte)i.Z, (byte)i.W);
    }

    public static byte[] SerialiseVector3(Vector3 v)
    {
        List<byte> data = new List<byte>();

        data.AddRange(BitConverter.GetBytes(v.X));
        data.AddRange(BitConverter.GetBytes(v.Y));
        data.AddRange(BitConverter.GetBytes(v.Z));

        return data.ToArray();
    }

    public static Vector3 DeserialiseVector3(byte[] data)
    {
        Vector3 result = Vector3.Zero;

        result.X = BitConverter.ToSingle(data.ToList().GetRange(0, 4).ToArray());
        result.Y = BitConverter.ToSingle(data.ToList().GetRange(4, 4).ToArray());
        result.Z = BitConverter.ToSingle(data.ToList().GetRange(8, 4).ToArray());

        return result;
    }

    public static byte[] SerialiseString(string v)
    {
        List<byte> data = new List<byte>();

        data.AddRange(Encoding.UTF8.GetBytes(v));

        return data.ToArray();
    }

    public static string DeserialiseString(byte[] data)
    {
        string result = "";

        result = Encoding.UTF8.GetString(data);

        return result;
    }

    public static MethodInfo GetExtensionMethod(string methodName)
    {
        return typeof(Extensions).GetMethod(methodName);
    }

    public static bool PointInQuad(Vector2 point, Vector4 quad)
    {
        if (point.X >= quad.X && point.X <= quad.Z)
        {
            if (point.Y >= quad.Y && point.Y <= quad.W)
            {
                return true;
            }
        }

        return false;
    }

    public static Vector3 FindClosestPointLineAxis(Vector3 position1, Vector3 direction1, Vector3 position2, Vector3 direction2)
    {
        Vector3 delta = position2 - position1;

        float a = direction1.Dot(direction1);
        float b = direction1.Dot(direction2);
        float c = direction2.Dot(direction2);
        float d = direction1.Dot(delta);
        float e = direction2.Dot(delta);

        float denominator = a * c - b * b;

        if (denominator == 0)
        {
            return position1;
        }

        float t1 = (b * e - c * d) / denominator;
        float t2 = (a * e - b * d) / denominator;

        Vector3 closestPoint1 = position1 + direction1 * t1;
        Vector3 closestPoint2 = position2 + direction2 * t2;

        Vector3 closestPoint = 0.5f * (closestPoint1 + closestPoint2);

        return closestPoint1;
    }

    public static Vector3 LinePlaneIntersection(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 lineStart, Vector3 lineDirection)
    {
        Vector3 planeNormal = ((point2 - point1).Cross(point3 - point1)).Normalized();

        float d = -planeNormal.Dot(point1);
        float t = -(planeNormal.Dot(lineStart) + d) / planeNormal.Dot(lineDirection);

        Vector3 intersectionPoint = lineStart + t * lineDirection;

        return intersectionPoint;
    }

    /// <summary>
    /// Checks if a ray is intersecting a given mesh. Avoid using this function too much as it is very slow. Use broad phase checks first such as a bounding box check when applicable.
    /// </summary>
    /// <param name="mesh">The mesh to check against</param>
    /// <param name="ray">The ray</param>
    /// <returns>True if the ray is intersecting the mesh</returns>
    public static bool RayMeshIntersection(Mesh mesh, Transform3D transform, Ray ray)
    {
        bool result = false;

        float[] vData = mesh.GetVertexPositions();

        Matrix4 model = transform.GetModelMatrix();
        for (int i = 0; i < mesh.indices.Length; i += 3)
        {
            uint i0 = mesh.indices[i] * 3;
            uint i1 = mesh.indices[i + 1] * 3;
            uint i2 = mesh.indices[i + 2] * 3;

            Vector3 p1 = new Vector3(vData[i0], vData[i0 + 1], vData[i0 + 2]);
            Vector3 p2 = new Vector3(vData[i1], vData[i1 + 1], vData[i1 + 2]);
            Vector3 p3 = new Vector3(vData[i2], vData[i2 + 1], vData[i2 + 2]);
            
            p1 = (model * new Vector4(p1, 1.0f)).Xyz;
            p2 = (model * new Vector4(p2, 1.0f)).Xyz;
            p3 = (model * new Vector4(p3, 1.0f)).Xyz;

            if (RayTriangleIntersection(p1, p2, p3, ray))
            {
                result = true;
                break;
            }
        }
        return result;
    }

    public static bool RayTriangleIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray)
    {
        bool result = false;

        // find line plane intersection point
        Vector3 intersectionPoint = LinePlaneIntersection(p1, p2, p3, ray.origin, ray.direction);

        Vector3 l1 = p2 - p1;
        Vector3 l2 = p3 - p2;
        Vector3 l3 = p1 - p3;

        float d1 = (intersectionPoint - p1).Dot(l1.Normalized());
        float d2 = (intersectionPoint - p2).Dot(l2.Normalized());
        float d3 = (intersectionPoint - p3).Dot(l3.Normalized());

        // check dot product sign
        if (d1 >= 0)
        {
            if (d2 >= 0)
            {
                if (d3 >= 0)
                {
                    result = true;
                }
            }
        }

        return result;
    }

    public static bool RayQuadIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray)
    {
        bool result = false;

        // find line plane intersection point
        Vector3 intersectionPoint = LinePlaneIntersection(p1, p2, p3, ray.origin, ray.direction);

        // find two perpendicular vectors of quad and do dot product comparisons
        Vector3 bottomLeftUp = p2 - p3;
        Vector3 bottomLeftRight = p1 - p3;
        Vector3 intersectionPointDifference = intersectionPoint - p3;

        float verticalProduct = intersectionPointDifference.Dot(bottomLeftUp.Normalized());
        float horizontalProduct = intersectionPointDifference.Dot(bottomLeftRight.Normalized());

        // check dot product size and sign
        if (verticalProduct >= 0)
        {
            if (verticalProduct <= bottomLeftUp.Length)
            {
                if (horizontalProduct >= 0)
                {
                    if (horizontalProduct <= bottomLeftRight.Length)
                    {
                        result = true;
                    }
                }
            }
        }

        return result;
    }

    public static float Dot(this Vector3 a, Vector3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    public static Vector3 Cross(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    }

    public static Vector3 Normalized(this Vector3 a)
    {
        if (a.Length < 0.001f){return Vector3.Zero;}
        return a / a.Length;
    }
}