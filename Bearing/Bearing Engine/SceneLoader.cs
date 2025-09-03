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
    private struct PresetRef
    {
        public List<PresetLineRef> lineRefs = new List<PresetLineRef>();
        public string presetName = "";
        public int startIndex = 0;
        public int length = 0;

        public PresetRef() { }
    }

    private struct PresetLineRef
    {
        public string replacement = "";
        public string newValue = "";

        public PresetLineRef() { }
    }

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

    public static GameObject LoadFromRealFile(string filepath, bool initialise = true)
    {
        string data = File.ReadAllText(filepath);

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


    /// <summary>
    /// If you find urself looking through the implementation of this function, gl.
    /// </summary>
    /// <param name="data">The json data to preprocess to parse the preset syntax</param>
    /// <returns></returns>
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
        bool waitingOnValue = false;

        char prevChar = ' ';

        List<PresetRef> presets = new List<PresetRef>();

        StringBuilder presetName = new StringBuilder();
        StringBuilder replacement = new StringBuilder();
        StringBuilder newValue = new StringBuilder();

        string cleaned = string.Join("",
    data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
        .Select(line => line.TrimStart()));

        // parsing
        onPreset = false;
        prevChar = ' ';
        foreach (char c in cleaned)
        {
            if (c == '#')
            {
                onPreset = true;

                presets.Add(new PresetRef()); 

                prevChar = c;
                continue;
            }

            if (!onPreset)
            {
                prevChar = c;
                continue;
            }

            if (c == '(' && !gotName)
            {
                onName = true;

                presetName.Clear();

                prevChar = c;
                continue;
            }

            if (c == ')' && !gotName)
            {
                onName = false;
                gotName = true;

                PresetRef pres = presets[presets.Count - 1];
                pres.presetName = presetName.ToString();
                presets[presets.Count - 1] = pres;

                prevChar = c;
                continue;
            }

            if (c == '|')
            {
                onNewValue = false;

                PresetRef pres = presets[presets.Count - 1];
                PresetLineRef lref = pres.lineRefs[pres.lineRefs.Count - 1];
                lref.newValue = newValue.ToString();
                pres.lineRefs[pres.lineRefs.Count-1] = lref;
                presets[presets.Count - 1] = pres;

                prevChar = c;
                continue;
            }

            if (onName)
            {
                presetName.Append(c);
                prevChar = c;
                continue;
            }

            if (c == ']' && prevChar == '[')
            {
                onNewValue = false;
                onPreset = false;

                PresetRef pres = presets[presets.Count - 1];
                pres.lineRefs.Clear();
                presets[presets.Count - 1] = pres;

                onName = false;
                waitingOnValue = false;
                onReplacement = false;
                gotName = false;

                prevChar = c;
                continue;
            }

            if (onReplacement)
            {
                if (c == ',')
                {
                    onReplacement = false;
                    waitingOnValue = true;
                    prevChar = c;

                    PresetRef pres = presets[presets.Count - 1];
                    PresetLineRef lref = pres.lineRefs[pres.lineRefs.Count - 1];
                    lref.replacement = replacement.ToString();
                    pres.lineRefs[pres.lineRefs.Count - 1] = lref;
                    presets[presets.Count - 1] = pres;

                    newValue.Clear();
                    continue;
                }

                replacement.Append(c);
                prevChar = c;
                continue;
            }

            if (waitingOnValue)
            {
                if (c != ' ' && c != '=')
                {
                    waitingOnValue = false;
                    onNewValue = true;
                    newValue.Append(c);
                    prevChar = c;
                    continue;
                }
            }

            if (c == ']' && prevChar == '|')
            {
                onNewValue = false;

                PresetRef pres = presets[presets.Count - 1];
                PresetLineRef lref = pres.lineRefs[pres.lineRefs.Count - 1];
                lref.newValue = newValue.ToString();
                pres.lineRefs[pres.lineRefs.Count - 1] = lref;
                presets[presets.Count-1] = pres;

                onName = false;
                waitingOnValue = false;
                onReplacement = false;
                gotName = false;

                onPreset = false;
                prevChar = c;
                continue;
            }

            if (prevChar == '|')
            {
                // if this triggers, because above it c == ']' exists
                // we know that c != ] and so we dont need to put that in the condition XD
                // jank ik ^^^ XDD

                onReplacement = true;

                PresetRef pres = presets[presets.Count - 1];
                pres.lineRefs.Add(new PresetLineRef());
                presets[presets.Count - 1] = pres;

                prevChar = c;

                replacement.Clear();
                replacement.Append(c);
                continue;
            }

            if (onNewValue)
            {
                newValue.Append(c);
                prevChar = c;
                continue;
            }

            if (c == '[')
            {
                onReplacement = true;

                PresetRef pres = presets[presets.Count - 1];
                pres.lineRefs.Add(new PresetLineRef());
                presets[presets.Count - 1] = pres;

                prevChar = c;

                replacement.Clear();
                continue;
            }
        }

        // first find start and length of preset snippet
        int pid = -1;
        int index = 0;
        foreach (char c in data)
        {
            if (c == '#')
            {
                pid++;
                PresetRef pres = presets[pid];
                pres.startIndex = index;
                presets[pid] = pres;
                onPreset = true;
            }

            if (c == ']' && (prevChar == '|' || prevChar == '[') && onPreset)
            {
                PresetRef pres = presets[pid];
                pres.length = index - pres.startIndex + 1; // +1 accounts for the last bracket
                presets[pid] = pres;
                onPreset = false;
            }

            if (c != ' ' && c != '\n' && c != '\r' && c != '\t')
                prevChar = c;

            index++;
        }

        // replacing text with parsed data
        string replaced = data;
        int distortion = 0;
        foreach (PresetRef p in presets)
        {
            string presetData = Resources.ReadAllText(Resource.FromPath($"./Resources/Scene/{p.presetName}.preset"));

            string cleanedPreset = string.Join("",
        presetData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Select(line => line.TrimStart()));

            cleanedPreset = string.Join("",cleanedPreset.Skip(1).SkipLast(1));

            foreach (PresetLineRef lr in p.lineRefs)
            {
                cleanedPreset = cleanedPreset.Replace(lr.replacement, lr.newValue);
            }

            replaced = replaced.Remove(p.startIndex+distortion, p.length);
            replaced = replaced.Insert(p.startIndex+distortion, cleanedPreset);

            distortion += cleanedPreset.Length - p.length;
        }

        return replaced.Trim('\uFEFF');
    }
}