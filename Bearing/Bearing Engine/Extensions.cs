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
}
