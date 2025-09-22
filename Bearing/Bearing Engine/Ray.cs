using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public struct Ray
{
    public Vector3 origin;
    public Vector3 direction;
    public Ray(Vector3 origin, Vector3 direction)
    {
        this.origin = origin;
        this.direction = direction.Normalized();
    }
}