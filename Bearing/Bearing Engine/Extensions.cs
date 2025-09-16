using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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

    public static float LerpAngle(this float a, float b, float t)
    {
        float delta = Repeat((b - a) + 180f, 360f) - 180f;
        return a + delta * t;
    }

    private static float Repeat(float t, float length)
    {
        return t - MathF.Floor(t / length) * length;
    }


    public static SKColor ToSKColour(this BearingColour c)
    {
        Vector4i i = (Vector4i)c.GetZeroTo255A();
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

        result.X = BitConverter.ToSingle(data.ToList().GetRange(0,4).ToArray());
        result.Y = BitConverter.ToSingle(data.ToList().GetRange(4,4).ToArray());
        result.Z = BitConverter.ToSingle(data.ToList().GetRange(8,4).ToArray());

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

        float a = Vector3.Dot(direction1, direction1);
        float b = Vector3.Dot(direction1, direction2);
        float c = Vector3.Dot(direction2, direction2);
        float d = Vector3.Dot(direction1, delta);
        float e = Vector3.Dot(direction2, delta);

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
        Vector3 planeNormal = Vector3.Cross(point2 - point1, point3 - point1).Normalized();

        float d = -Vector3.Dot(planeNormal, point1);
        float t = -(Vector3.Dot(planeNormal, lineStart) + d) / Vector3.Dot(planeNormal, lineDirection);

        Vector3 intersectionPoint = lineStart + t * lineDirection;

        return intersectionPoint;
    }
}