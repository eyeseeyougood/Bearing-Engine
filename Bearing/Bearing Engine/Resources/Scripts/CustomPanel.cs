using Bearing;

public class CustomPanel : UIPanel
{
	public int borderWidth { get; set; } = 5;

	public CustomPanel(params object[] meta) : base(meta)
    {
        material = new Material()
        {
            shader = new Shader("res/customPanelUI.vert", "res/customPanelUI.frag"),
        };
    }

    public override void OnTick(float dt)
    {
        base.OnTick(dt);

        material.PassValue("mainColour", GetThemeValue<BearingColour>("panelBG").zeroToOne);
        material.PassValue("outlineColour", GetThemeValue<BearingColour>("panelOutline").zeroToOne);
        material.PassValue("borderWidth", borderWidth);
    }
}