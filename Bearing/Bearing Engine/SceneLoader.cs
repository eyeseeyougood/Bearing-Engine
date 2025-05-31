using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Bearing;

public static class SceneLoader
{
    public static void Tick() { }

    public static GameObject LoadFromFile(string filepath)
    {
        string data = File.ReadAllText(filepath);

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new ComponentConverter(), new ParameterConverter() }
        };

        GameObject root = JsonConvert.DeserializeObject<GameObject>(data, settings);
        root.Load();

        return root;
    }
}