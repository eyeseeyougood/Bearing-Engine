using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class Logger
{
    private static Dictionary<string, int> counts = new Dictionary<string, int>();
    public static void Count(string key)
    {
        if (!counts.ContainsKey(key))
            counts.Add(key, 0);

        counts[key]++;
        Log($"Counted \"{key}\": {counts[key]} times");
    }

    public static void Log(object message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[{MathF.Round(Time.now, 3)}] "+message);
    }

    public static void Log(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"[{MathF.Round(Time.now, 3)}] " + message);
    }

    public static void LogError(object message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{MathF.Round(Time.now, 3)}] ERROR: "+message);
    }
}