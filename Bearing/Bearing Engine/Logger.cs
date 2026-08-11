using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class Logger
{
    private static Dictionary<string, int> counts = new Dictionary<string, int>();
    public static Action<object, ConsoleColor> onLog = (i,j)=>{};

    public static void Count(string key)
    {
        if (!counts.ContainsKey(key))
            counts.Add(key, 0);

        counts[key]++;
        Log($"Counted \"{key}\": {counts[key]} times");
    }

    private static Dictionary<string, float> startTimes = new Dictionary<string, float>();
    private static Dictionary<string, float> totalTime = new Dictionary<string, float>();
    private static Dictionary<string, int> exeTimes = new Dictionary<string, int>();
    public static void MeasureStart(string taskName = "Dafault Task")
    {
        startTimes.Add(taskName, Time.now);

        if (!exeTimes.ContainsKey(taskName))
            exeTimes.Add(taskName, 0);

        exeTimes[taskName]++;
    }

    public static void MeasureEnd(string taskName = "Default Task")
    {
        float diff = Time.now - startTimes[taskName];

        startTimes.Remove(taskName);

        if (!totalTime.ContainsKey(taskName))
            totalTime.Add(taskName, 0);

        totalTime[taskName] += diff;

        Log($"'{taskName}' Task took {diff}s, and ran {exeTimes[taskName]} times.");

        Log($"Total time consumption of '{taskName}' Task is {totalTime[taskName]}s");
    }

    public static void Log(object message)
    {
        Log(message, ConsoleColor.White);
    }

    public static void Log(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"[{MathF.Round(Time.now, 3)}] " + message);
        onLog.Invoke(message, color);
    }

    public static void LogError(object message)
    {
        Log(message, ConsoleColor.Red);
    }
}