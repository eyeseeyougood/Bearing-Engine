using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Bearing;

public static class SceneLoader
{
    public static void Tick()
    {

    }

    public static GameObject LoadFromFile(string filepath)
    {
        return LoadFromFile(File.ReadAllText(filepath).Replace("\r", "").Split('\n'));
    }
    public static GameObject LoadFromFile(string[] data)
    {
        return null;
    }
}