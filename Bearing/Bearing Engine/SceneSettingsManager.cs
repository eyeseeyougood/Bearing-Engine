using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class SceneSettingsManager
{
    public static string settings = "";

    public static void Init()
    {
        settings = Resources.ReadAllText(Resource.FromPath("./Resources/Scene/settings.txt"));
    }

    /// <summary>
    /// This fetches all of the settings under a section header from the scene settings file.
    /// </summary>
    /// <param name="section">The desired section title.</param>
    /// <returns>The settingsA dictionary of setting names and their values as strings. If the section title isn't found then returns null.</returns>
    public static Dictionary<string, string>? GetSettings(string section)
    {
        // quick and shitty check for if the secion exists (not reliable)
        if (!settings.Contains(section))
        {
            Logger.LogError("Invalid scene settings: Requested section is missing from the settings file!");
            return null;
        }

        string sectionHeader = $"--- {section} ---";
        string[] lines = settings.Substring(settings.IndexOf(sectionHeader)).Split('\n').Skip(1).ToArray();

        Dictionary<string, string> result = new Dictionary<string, string>();

        foreach (string line in lines)
        {
            if (line == sectionHeader)
                break;

            string filteredLine = line.Trim().Replace(" ","");

            string[] parts = filteredLine.Split("=");

            if (parts.Length != 2)
                Logger.LogError("Invalid scene settings: Incorrect number of setting parts!");

            result.Add(parts[0], parts[1]);
        }

        return result;
    }
}