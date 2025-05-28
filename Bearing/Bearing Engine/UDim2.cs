using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bearing;

public struct UDim2
{
    public Vector2 scale = Vector2.Zero;
    public Vector2 offset = Vector2.Zero;

    public static readonly UDim2 Zero = new UDim2(0, 0, 0, 0);
    public static readonly UDim2 One = new UDim2(1, 1, 0, 0);

    public override string ToString()
    {
        return $"({scale},{offset})";
    }

    public UDim2() { }
    public UDim2(Vector2 scale) { this.scale = scale; }
    public UDim2(Vector2 scale, Vector2 offset) { this.scale = scale; this.offset = offset; }
    public UDim2(float scaleX, float scaleY) { scale = new Vector2(scaleX, scaleY); }
    public UDim2(float scaleX, float scaleY, float offsetX, float offsetY) { scale = new Vector2(scaleX, scaleY); offset = new Vector2(offsetX, offsetY); }
}