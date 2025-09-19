using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class Time
{
    public static float now
    {
        get
        {
            return (float)(sw.Elapsed.TotalMilliseconds/1000d);
        }
    }

    private static Stopwatch sw;
    public static float evaluationTime = 0.05f;

    public static void Init()
    {
        sw = Stopwatch.StartNew();
    }

    public static async Task WaitTillTime(float time)
    {
        while (sw.Elapsed.TotalSeconds < time)
        {
            await Task.Delay((int)(evaluationTime*1000));
        }
    }
}