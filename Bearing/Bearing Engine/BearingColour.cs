using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Bearing;

public struct BearingColour
{
    public static readonly BearingColour Transparent = new BearingColour() { zeroToOne = new Vector4(0,0,0,0) };
    public static readonly BearingColour Black = new BearingColour() { zeroToOne = new Vector4(0,0,0,1) };
    public static readonly BearingColour BrightBlack = new BearingColour() { zeroToOne = new Vector4(0.1f, 0.1f, 0.1f, 1) };
    public static readonly BearingColour White = new BearingColour() { zeroToOne = new Vector4(1f, 1f, 1f, 1f) };
    public static readonly BearingColour DarkWhite = new BearingColour() { zeroToOne = new Vector4(0.82f, 0.82f, 0.82f, 1f) };
    public static readonly BearingColour LightGray = new BearingColour() { zeroToOne = new Vector4(0.75f, 0.75f, 0.75f, 1f) };
    public static readonly BearingColour Gray = new BearingColour();
    public static readonly BearingColour DarkGray = new BearingColour() { zeroToOne = new Vector4(0.25f, 0.25f, 0.25f, 1f) };

    public static readonly BearingColour Red = new BearingColour() { zeroToOne = new Vector4(1, 0, 0, 1) };
    public static readonly BearingColour Green = new BearingColour() { zeroToOne = new Vector4(0, 1, 0, 1) };
    /// <summary>
    /// In memory of Mihran Khachatryan. A great friend who will never be forgotten.
    /// </summary>
    public static readonly BearingColour OneBlue = new BearingColour() { zeroToOne = new Vector4(0,0,1,1) };
    public static readonly BearingColour LightBlue = new BearingColour() { zeroToOne = new Vector4(0.1f,0.3f,0.8f,1) };

    public Vector4 zeroToOne { get; set; } = new Vector4(0.5f, 0.5f, 0.5f, 1f); // default to gray

    public BearingColour() { }

    public static BearingColour FromZeroToOne(Vector3 zeroToOne)
    {
        return new BearingColour() { zeroToOne = new Vector4(zeroToOne, 1f) };
    }

    public static BearingColour FromZeroToOne(Vector4 zeroToOne)
    {
        return new BearingColour() { zeroToOne = zeroToOne };
    }

    public static BearingColour FromZeroTo255(Vector3 zeroTo255)
    {
        return new BearingColour() { zeroToOne = new Vector4(zeroTo255/255f, 1f) };
    }

    public static BearingColour FromZeroTo255(Vector4 zeroTo255)
    {
        return new BearingColour() { zeroToOne = zeroTo255 / 255f };
    }

    public Vector3 GetZeroTo255()
    {
        return zeroToOne.Xyz * 255f;
    }

    public Vector4 GetZeroTo255A()
    {
        return zeroToOne * 255f;
    }

    public Vector3 GetZeroToOne()
    {
        return zeroToOne.Xyz;
    }

    public Vector4 GetZeroToOneA()
    {
        return zeroToOne;
    }

    public static bool operator ==(BearingColour own, BearingColour other)
    {
        return own.zeroToOne == other.zeroToOne;
    }

    public static bool operator !=(BearingColour own, BearingColour other)
    {
        return own.zeroToOne != other.zeroToOne;
    }
}
