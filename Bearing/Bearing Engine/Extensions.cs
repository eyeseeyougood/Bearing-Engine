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

    public static float[] ToFloatArray(this Matrix4 mat)
    {
        return new float[] {
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
            };
    }

    public static object[] ToObjectArray(this Matrix4 mat)
    {
        return new object[] {
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
            };
    }

    public static T? GetMeta<T>(this IMetadata m, int index = 0)
    {
        if (index >= m.metadata.Length || index < 0)
            return default(T);

        return (T)m.metadata[index];
    }

    public static BulletSharp.Math.Vector3 ToBulletVector(this Vector3 vector)
    {
        return new BulletSharp.Math.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static Vector2 ToTKVector(this System.Numerics.Vector2 vector)
    {
        return new Vector2(vector.X, vector.Y);
    }

    public static Vector2 ToTKVector(this Box2D.NET.B2Vec2 vector)
    {
        return new Vector2(vector.X, vector.Y);
    }

    public static Vector3 ToTKVector(this BulletSharp.Math.Vector3 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }

    public static Box2D.NET.B2Vec2 ToB2Vector(this Vector2 vector)
    {
        return new Box2D.NET.B2Vec2(vector.X, vector.Y);
    }

    public static System.Numerics.Vector2 ToSystemVector(this Vector2 vector)
    {
        return new System.Numerics.Vector2(vector.X, vector.Y);
    }

    public static System.Numerics.Vector3 ToSystemVector(this Vector3 vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static float LerpAngle(this float a, float b, float t)
    {
        float delta = Repeat((b - a) + 180f, 360f) - 180f;
        return a + delta * t;
    }

    public static Quaternion LookAt(Vector3 start, Vector3 target)
    {
        Vector3 diff = (start - target).Normalized();

        Vector3 right = Vector3.Cross(Vector3.UnitY, diff);
        Vector3 up = Vector3.Cross(diff, right);

        Quaternion rot = new Matrix3(right.X, right.Y, right.Z, up.X, up.Y, up.Z, diff.X, diff.Y, diff.Z).ExtractRotation();

        return rot;
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

    public static Ray GetRayFromMouse()
    {
        Vector2 mp = Input.GetMousePosition();

        float mouse_x = mp.X;
        float mouse_y = mp.Y;

        float width = Game.instance.ClientSize.X;
        float height = Game.instance.ClientSize.Y;

        float x = (2.0f * mouse_x) / width - 1.0f;
        float y = 1.0f - (2.0f * mouse_y) / height;
        float z = 1.0f;
        Vector3 ray_nds = new Vector3(x, y, z);

        Vector4 ray_clip = new Vector4(ray_nds.X, ray_nds.Y, -1.0f, 1.0f);

        Vector4 ray_eye = ray_clip * Game.instance.camera.GetProjectionMatrix().Inverted();

        ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0.0f);

        Vector3 ray_wor = (ray_eye * Game.instance.camera.GetViewMatrix().Inverted()).Xyz;

        ray_wor.Normalize();

        return new Ray(Game.instance.camera.Position, ray_wor);
    }

    /// <summary>
    /// This function assumes the gameobject (go) has a MeshRenderer component, with a mesh already assigned.
    /// Note: this function doesn't check if there are other objects which the mouse is also over.
    /// </summary>
    public static bool IsMouseOverGameObject(GameObject go)
    {
        Ray mouseRay = GetRayFromMouse();

        return RayMeshIntersection((Mesh3D)go.GetComponent<MeshRenderer>().GetMesh(), (Transform3D)go.transform, mouseRay);
    }

    /// <summary>
    /// This function makes use of the RayMeshIntersection function, but first applies a simple broad-phase check agains the AABB of the mesh.
    /// It is much better to use this function than just RayMeshIntersection alone.
    /// </summary>
    public static bool RayMeshIntersectionFAST(Mesh3D mesh, Transform3D transform, Ray ray)
    {
        bool result = false;

        (Vector3, Vector3) bbSize = mesh.GetBoundingBox();

        Vector3 pos = transform.position;

        if (Extensions.RayAABBIntersection(ray, bbSize.Item1 + pos, bbSize.Item2 + pos, out float d, out Vector3 p))
        {
            if (Extensions.RayMeshIntersection(mesh, transform, ray))
            {
                result = true;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a ray is intersecting a given mesh. Avoid using this function too much as it is very slow.
    /// Use broad-phase checks first such as a bounding box check when applicable (This is available in the FAST version of this function).
    /// </summary>
    /// <param name="mesh">The mesh to check against</param>
    /// <param name="ray">The ray</param>
    /// <returns>True if the ray is intersecting the mesh</returns>
    public static bool RayMeshIntersection(Mesh3D mesh, Transform3D transform, Ray ray)
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
            
            p1 = (new Vector4(p1, 1.0f) * model).Xyz;
            p2 = (new Vector4(p2, 1.0f) * model).Xyz;
            p3 = (new Vector4(p3, 1.0f) * model).Xyz;

            if (RayTriangleIntersection(p1, p2, p3, ray))
            {
                result = true;
                break;
            }
        }
        return result;
    }

    // Möller–Trumbore implementation - more accurate than the previous method
    public static bool RayTriangleIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray)
    {
        const float EPSILON = 0.000001f;

        Vector3 edge1 = p2 - p1;
        Vector3 edge2 = p3 - p1;

        Vector3 h = Vector3.Cross(ray.direction, edge2);
        float a = Vector3.Dot(edge1, h);

        if (MathF.Abs(a) < EPSILON)
            return false;

        float f = 1.0f / a;
        Vector3 s = ray.origin - p1;

        float u = f * Vector3.Dot(s, h);
        if (u < 0.0f || u > 1.0f)
            return false;

        Vector3 q = Vector3.Cross(s, edge1);

        float v = f * Vector3.Dot(ray.direction, q);
        if (v < 0.0f || u + v > 1.0f)
            return false;

        float t = f * Vector3.Dot(edge2, q);

        return t > EPSILON;
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

    public static bool RayAABBIntersection(Ray ray, Vector3 aabbMin, Vector3 aabbMax, out float intersectionDepth, out Vector3 intersectionPoint)
    {
        Vector3 p = ray.origin;
        Vector3 d = ray.direction;

        intersectionPoint = new Vector3(float.NaN, float.NaN, float.NaN);
        intersectionDepth = 0.0f;             // Start with the minimum distance (can be -FLT_MAX for entire ray)
        float tmax = float.MaxValue;    // Maximum allowable distance for the ray (segment length or ∞)

        // Iterate over each axis (x, y, z)
        for (int i = 0; i < 3; i++)
        {
            // If the ray is parallel to the slab (AABB plane pair)
            if (Math.Abs(d[i]) < float.Epsilon)
            {
                // If the origin is outside the slab, there's no intersection
                if (p[i] < aabbMin[i] || p[i] > aabbMax[i])
                    return false;
            }
            else
            {
                // Compute the intersection t-values for the near and far planes of the slab
                float ood = 1.0f / d[i];
                float t1 = (aabbMin[i] - p[i]) * ood;
                float t2 = (aabbMax[i] - p[i]) * ood;

                // Ensure t1 is the intersection with the near plane, and t2 with the far plane
                if (t1 > t2)
                {
                    float temp = t1; // Swap t1 and t2
                    t1 = t2;
                    t2 = temp;
                }

                // Update intersectionDepth and tmax to compute the intersection interval
                intersectionDepth = Math.Max(intersectionDepth, t1);
                tmax = Math.Min(tmax, t2);

                // If the interval becomes invalid, there is no intersection
                if (intersectionDepth > tmax)
                    return false;
            }
        }

        // If we reach here, the ray intersects the AABB on all 3 axes
        intersectionPoint = p + d * intersectionDepth; // Compute the intersection point
        return true;
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