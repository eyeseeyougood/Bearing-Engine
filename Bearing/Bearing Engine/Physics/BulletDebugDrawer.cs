using BulletSharp;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace Bearing;

public class BulletDebugDrawer : DebugDraw
{
    private DebugDrawModes _debugMode = DebugDrawModes.None;

    public override DebugDrawModes DebugMode
    {
        get => _debugMode;
        set => _debugMode = value;
    }

    public override void Draw3DText(ref BulletSharp.Math.Vector3 location, string textString)
    {
        Gizmos.CreateSphere(location.ToTKVector(), 0.1f, 0.03f, BearingColour.LightBlue);
        // Not used by most Bullet demos — optional
    }

    public override void DrawLine(ref BulletSharp.Math.Vector3 from, ref BulletSharp.Math.Vector3 to, ref BulletSharp.Math.Vector3 color)
    {
        // Convert BulletSharp.Math.Vector3 to OpenTK.Mathematics.Vector3
        var fromVec = new Vector3(from.X, from.Y, from.Z);
        var toVec = new Vector3(to.X, to.Y, to.Z);
        var colorVec = new Vector3(color.X, color.Y, color.Z);

        Gizmos.CreateVector(toVec - fromVec, fromVec, 0.03f, BearingColour.FromZeroToOne(colorVec));
    }

    public override void ReportErrorWarning(string warningString)
    {
        Logger.LogError(warningString);
    }
}
