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

    public static GameObject LoadFromFile(string filepath, bool initialise = true)
    {
        string data = Resources.ReadAllText(Resource.FromPath(filepath));

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new ComponentConverter() }
        };

        GameObject root = JsonConvert.DeserializeObject<GameObject>(data, settings);

        if (initialise)
            root.Load();

        return root;
    }
}