using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class Logger
{
    public static void Log(string message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[{MathF.Round(Time.now, 3)}] "+message);
    }

    public static void Log(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"[{MathF.Round(Time.now, 3)}] " + message);
    }

    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{MathF.Round(Time.now, 3)}] ERROR: "+message);
    }
}