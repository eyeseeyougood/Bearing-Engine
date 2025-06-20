using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class Time
{
    public static float now;

    private static double currTime;

    public static void Tick(double dt)
    {
        currTime += dt;
        now = (float)currTime;
    }
}
