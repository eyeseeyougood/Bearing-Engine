using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class BearingColour
{
    Vector4 zeroToOne;

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
}
