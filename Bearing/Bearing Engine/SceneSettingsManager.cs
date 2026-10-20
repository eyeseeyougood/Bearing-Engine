using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class SceneSettingsManager
{
    public static string settings = "";
    private static bool settingsExist = false;

    public static void Init()
    {
        EmbeddedResource res = EmbeddedResource.FromPath("./Resources/Scene/settings.txt");

        if (EmbeddedResource.Exists(res))
        {
            settings = Resources.ReadAllText(res);
            settingsExist = true;
        }
    }

    /// <summary>
    /// This fetches all of the settings under a section header from the scene settings file.
    /// </summary>
    /// <param name="section">The desired section title.</param>
    /// <returns>A dictionary of setting names and their values as strings. If the section title isn't found then returns null.</returns>
    public static Dictionary<string, string>? GetSettings(string section)
    {
        if (!SettingsExist(section))
        {
            Logger.LogError($"Invalid scene settings: Requested section '{section}' is missing from the settings file!");
            return null;
        }

        string sectionHeader = $"--- {section} ---";
        string[] lines = settings.Substring(settings.IndexOf(sectionHeader)).Split('\n').Skip(1).ToArray();

        Dictionary<string, string> result = new Dictionary<string, string>();

        foreach (string line in lines)
        {
            string filteredLine = line.ReplaceLineEndings().Trim();
            
            if (filteredLine == sectionHeader)
                break;

            string[] parts = filteredLine.Split("=");

            if (parts.Length != 2)
                throw new Exception("Invalid scene settings: Incorrect number of setting parts!");

            result.Add(parts[0].Trim(), parts[1].Trim());
        }

        return result;
    }

    public static bool SettingsExist(string section)
    {
        // quick and shitty check for if the secion exists (not reliable)
        // TODO: improve, this can be broken in the condition that the given section doesnt exist, but a setting or value of a setting in
        // another section is named the same as the given section header
        string sectionHeader = $"--- {section} ---";
        return settingsExist && settings.Contains(sectionHeader);
    }
}