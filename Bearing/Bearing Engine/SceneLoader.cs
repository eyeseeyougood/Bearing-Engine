using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bearing;

public static class SceneLoader
{
    public static void Tick() { }

    public static GameObject LoadFromFile(string filepath, bool initialise = true)
    {
        string data = Resources.ReadAllText(Resource.FromPath(filepath));

        data = Preprocess(data); // stuff like presets

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new ComponentConverter() }
        };

        GameObject root = JsonConvert.DeserializeObject<GameObject>(data, settings);

        if (initialise)
            root.Load();

        return root;
    }

    private static string Preprocess(string data)
    {
        /*
                #PRESET(button)
                [
                    "hello!!!", = "This button was made with a preset!"|
                ],*/

        bool onPreset = false;
        bool onName = false;
        bool onReplacement = false;
        bool onNewValue = false;
        bool gotName = false;

        StringBuilder presetName = new StringBuilder();
        StringBuilder replacement = new StringBuilder();
        StringBuilder newValue = new StringBuilder();

        foreach (char c in data)
        {
            if (c == '#')
            {
                onPreset = true;
                continue;
            }

            if (!onPreset)
                continue;

            if (c == '(' && !gotName)
            {
                onName = true;
                continue;
            }

            if (c == ')' && !gotName)
            {
                onName = false;
                gotName = true;
                continue;
            }

            if (onName)
            {
                presetName.Append(c);
                continue;
            }

            if (c == '[')
        }
    }
}