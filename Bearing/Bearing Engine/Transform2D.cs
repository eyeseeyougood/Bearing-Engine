using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Bearing;

public class Transform2D : Transform
{
    private Vector2 _position = Vector2.Zero;
    private float _rotation = 0;
    private Vector2 _scale = Vector2.One;

    public Vector2 position
    {
        get { return _position; }
        set
        {
            _position = value;
            InvokePositionChanged();
            InvokeTransformChanged();
        }
    }

    public float rotation
    {
        get { return _rotation; }
        set
        {
            _rotation = value;
            InvokeRotationChanged();
            InvokeTransformChanged();
        }
    }

    public Vector2 scale
    {
        get { return _scale; }
        set
        {
            _scale = value;
            InvokeScaleChanged();
            InvokeTransformChanged();
        }
    }

    public Vector2 GetRight()
    {
        Vector2 result = new Vector2(
            MathF.Cos(rotation),
            MathF.Sin(rotation)
            );

        return result.Normalized();
    }

    public Vector2 GetUp()
    {
        Vector2 result = new Vector2(
            MathF.Cos(rotation+90),
            MathF.Sin(rotation+90)
            );

        return result.Normalized();
    }
}