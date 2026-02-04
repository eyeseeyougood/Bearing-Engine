using BulletSharp;
using OpenTK.Mathematics;

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
    }

    public override void DrawLine(ref BulletSharp.Math.Vector3 from, ref BulletSharp.Math.Vector3 to, ref BulletSharp.Math.Vector3 color)
    {
        var fromVec = from.ToTKVector();
        var toVec = to.ToTKVector();
        var colorVec = color.ToTKVector();

        Gizmos.CreateVector(toVec - fromVec, fromVec, 0.03f, BearingColour.FromZeroToOne(colorVec));
    }

    public override void ReportErrorWarning(string warningString)
    {
        Logger.LogError(warningString);
    }
}
