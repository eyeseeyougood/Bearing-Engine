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
        selection = null,
        labelText = null,
        buttonUpBackground = null,
        buttonDownBackground = null,

        buttonHoverBackground = null,
        verticalScrollBG = null,
        uiPanelBG = null,

        buttonHoverAudio = null,
        buttonDownAudio = null,
        buttonUpAudio = null
    };

    public BearingColour? selection = BearingColour.LightBlue;
    public BearingColour? labelText = BearingColour.Black;
    public BearingColour? buttonUpBackground = BearingColour.LightGray;
    public BearingColour? buttonDownBackground = BearingColour.Gray;
    public BearingColour? buttonHoverBackground = BearingColour.DarkWhite;
    public BearingColour? verticalScrollBG = BearingColour.DarkGray;
    public BearingColour? uiPanelBG = BearingColour.LightGray;

    // audio
    public string buttonHoverAudio = "None";
    public string buttonDownAudio = "None";
    public string buttonUpAudio = "None";

    public object Clone()
    {
        UITheme result;

        result = new UITheme();

        result.selection = selection;
        result.labelText = labelText;
        result.buttonUpBackground = buttonUpBackground;
        result.buttonDownBackground = buttonDownBackground;
        result.buttonHoverBackground = buttonHoverBackground;
        result.verticalScrollBG = verticalScrollBG;
        result.uiPanelBG = uiPanelBG;

        // audio
        result.buttonHoverAudio = buttonHoverAudio;
        result.buttonDownAudio = buttonDownAudio;
        result.buttonUpAudio = buttonUpAudio;

        return result;
    }
}