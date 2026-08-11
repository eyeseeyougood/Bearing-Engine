using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public class UITheme : ICloneable
{
    public static readonly UITheme Empty = new UITheme()
    {
        colours = new Dictionary<string, BearingColour>(),
        audios = new Dictionary<string, string>()
    };

    private Dictionary<string, BearingColour> colours = new Dictionary<string, BearingColour>()
    {
        {"selection",                       BearingColour.LightBlue},
        {"labelText",                       BearingColour.Black},
        {"buttonUpBackground",              BearingColour.LightGray},
        {"buttonDownBackground",            BearingColour.Gray},
        {"buttonHoverBackground",           BearingColour.DarkWhite},
        {"verticalScrollBG",                BearingColour.DarkGray},
        {"panelBG",                       BearingColour.LightGray},
        {"sliderBackground",                BearingColour.DarkGray},
        {"sliderFill",                      BearingColour.LightGray},
    };
    private Dictionary<string, string> audios = new Dictionary<string, string>();
    // built-in audio references:
    // buttonHoverAudio
    // buttonDownAudio
    // buttonUpAudio

    public BearingColour? GetColour(string uiState)
    {
        if (colours.ContainsKey(uiState))
            return colours[uiState];

        return null;
    }

    public Resource? GetAudio(string audio)
    {
        if (audios.ContainsKey(audio))
            return Resource.FromPath(audios[audio]);

        return null;
    }

    public void SetColour(string uiState, BearingColour? colour)
    {
        if (colour is not null)
            SetColour(uiState, (BearingColour)colour);
        else
            RemoveColour(uiState);
    }

    public void SetColour(string uiState, BearingColour colour)
    {
        if (!colours.ContainsKey(uiState))
            colours.Add(uiState, BearingColour.Black);

        colours[uiState] = colour;
    }

    public void SetAudio(string audio, Resource? audioResource)
    {
        if (audioResource is null)
        {
            RemoveAudio(audio);
            return;
        }

        if (!audios.ContainsKey(audio))
            audios.Add(audio, "");

        audios[audio] = audioResource.fullpath;
    }

    public void RemoveColour(string uiState)
    {
        if (colours.ContainsKey(uiState))
            colours.Remove(uiState);
    }

    public void RemoveAudio(string audio)
    {
        if (audios.ContainsKey(audio))
            audios.Remove(audio);
    }

    public bool ContainsColour(string uiState)
    {
        return colours.ContainsKey(uiState);
    }

    public bool ContainsAudio(string audio)
    {
        return audios.ContainsKey(audio);
    }

    public object Clone()
    {
        UITheme result = new UITheme();

        result.colours = colours.ToDictionary();
        result.audios = audios.ToDictionary();

        return result;
    }
}