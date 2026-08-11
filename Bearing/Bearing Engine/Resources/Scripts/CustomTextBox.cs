using OpenTK.Mathematics;
using Bearing;

public class CustomTextBox : UITextBox
{
    public int borderWidth = 5;

    public CustomTextBox(params object[] meta) : base(meta)
    {
        material = new Material()
        {
            shader = new Shader("res/customPanelUI.vert", "res/customPanelUI.frag"),
        };
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.SetShaderParameter("outlineColour", GetThemeValue<BearingColour>("panelOutline").zeroToOne);
        material.SetShaderParameter("borderWidth", borderWidth);
    }
}