using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class UITheme : ICloneable
{
    public enum ThemeExportColourPrecisionMode
    {
        ZeroToOne = 0,
        ZeroTo255 = 1
    }

    public static readonly UITheme Empty = new UITheme()
    {
        colours = new Dictionary<string, BearingColour>(),
        audios = new Dictionary<string, Resource?>()
    };

    private Dictionary<string, BearingColour> colours = new Dictionary<string, BearingColour>()
    {
        {"selection",                       BearingColour.LightBlue},
        {"labelText",                       BearingColour.Black},
        {"buttonUpBackground",              BearingColour.LightGray},
        {"buttonDownBackground",            BearingColour.Gray},
        {"buttonHoverBackground",           BearingColour.DarkWhite},
        {"verticalScrollBG",                BearingColour.DarkGray},
        {"panelBG",                         BearingColour.LightGray},
        {"sliderBackground",                BearingColour.DarkGray},
        {"sliderFill",                      BearingColour.LightGray},
    };

    private Dictionary<string, Resource?> audios = new Dictionary<string, Resource?>()
    {
        {"buttonHoverAudio",                null},
        {"buttonDownAudio",                 null},
        {"buttonUpAudio",                   null},
    };

    public BearingColour? GetColour(string alias)
    {
        if (colours.ContainsKey(alias))
            return colours[alias];

        return null;
    }

    public Resource? GetAudio(string alias)
    {
        if (audios.ContainsKey(alias))
            return audios[alias];

        return null;
    }

    public void SetColour(string alias, BearingColour? colour)
    {
        if (colour is not null)
            SetColour(alias, (BearingColour)colour);
        else
            RemoveColour(alias);
    }

    public void SetColour(string alias, BearingColour colour)
    {
        if (!colours.ContainsKey(alias))
            colours.Add(alias, BearingColour.Black);

        colours[alias] = colour;
    }

    public void SetAudio(string alias, Resource? audioResource)
    {
        if (!audios.ContainsKey(alias))
            audios.Add(alias, audioResource);
        else
            audios[alias] = audioResource;
    }

    public void RemoveColour(string alias)
    {
        if (colours.ContainsKey(alias))
            colours.Remove(alias);
    }

    public void RemoveAudio(string audio)
    {
        if (audios.ContainsKey(audio))
            audios.Remove(audio);
    }

    public bool ContainsColour(string alias)
    {
        return colours.ContainsKey(alias);
    }

    public bool ContainsAudio(string alias)
    {
        return audios.ContainsKey(alias);
    }
    ///<summary>
    ///Note: Audios will only be exported if they are embedded resources, external resources will fail to export.
    ///</summary>
    public string ExportValues(ThemeExportColourPrecisionMode exportMode = ThemeExportColourPrecisionMode.ZeroTo255)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("colours:");
        foreach (var kvp in colours)
        {
            switch (exportMode)
            {
                case ThemeExportColourPrecisionMode.ZeroToOne:
                    sb.AppendLine(kvp.Key+":"+kvp.Value.zeroToOne.X+","+kvp.Value.zeroToOne.Y+","+kvp.Value.zeroToOne.Z+","+kvp.Value.zeroToOne.W);
                    break;
                case ThemeExportColourPrecisionMode.ZeroTo255:
                    OpenTK.Mathematics.Vector4 zt255 = kvp.Value.GetZeroTo255A();
                    sb.AppendLine(kvp.Key+":"+zt255.X+","+zt255.Y+","+zt255.Z+","+zt255.W);
                    break;
            }
        }
        sb.AppendLine("audios:");
        foreach (var kvp in audios)
        {
            if (kvp.Value is EmbeddedResource embeddedResource)
                sb.AppendLine(kvp.Key+":"+embeddedResource.fullpath);
            else if (kvp.Value is null)
                sb.AppendLine(kvp.Key+":NULL");
        }
        return sb.ToString();
    }

    ///<summary>
    ///WARNING: THIS FUNCTION IS NOT ATOMIC. IF THE 'valueText' IS INVALID IMPORTING WILL FAIL AND ANY VALUES STORED BY THIS THEME WILL BE CLEARED!
    ///</summary>
    public void ImportValues(string valueText, ThemeExportColourPrecisionMode precision = ThemeExportColourPrecisionMode.ZeroTo255)
    {
        colours.Clear();
        audios.Clear();

        StringBuilder sb = new StringBuilder();
        valueText = valueText.Replace("colours:\n","");
        bool stage2 = false;
        foreach (string line in valueText.Split("\n"))
        {
            if (line == "")
                continue;
            if (line == "audios:")
            {
                stage2 = true;
                continue;
            }
            if (!stage2)
            {
                string key = line.Split(":").First();
                string value = line.Split(":").Last();
                string[] parts = value.Split(",");

                OpenTK.Mathematics.Vector4 colour = new OpenTK.Mathematics.Vector4(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3])
                    );

                int mod = precision == ThemeExportColourPrecisionMode.ZeroTo255 ? 255 : 1;
                colours.Add(key, BearingColour.FromZeroToOne(colour/mod));
            }
            else
            {
                string key = line.Split(":").First();
                string path = line.Split(":").Last();

                if (path == "NULL")
                {
                    audios.Add(key, null);
                    continue;
                }

                audios.Add(key, EmbeddedResource.FromPath(path));
            }
        }
    }

    public object Clone()
    {
        UITheme result = new UITheme();

        result.colours = colours.ToDictionary();
        result.audios = audios.ToDictionary();

        return result;
    }
}