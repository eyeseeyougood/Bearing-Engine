using OpenTK.Mathematics;
using Bearing;

public class ShaderView : Component
{
	private int parent;

	public ShaderView(int parent) { this.parent = parent; }

    public override void OnLoad()
    {
    	ShaderImage image = new ShaderImage();
    	image.parent = parent;
    	image.anchor = new Vector2(1,0);
    	image.position = new UDim2(1,0.3f,-5,5);
    	image.size = new UDim2(0.2f,0.2f,-10,-10);
    	gameObject.AddComponent(image);

    	GameObject go = new GameObject();
    	go.parent = Game.instance.root;
    	go.AddMeta("___HIDEFROMHIERARCHY___");
    	go.AddComponent(new ShaderRenderer(image));


        CustomTextBox codeBox = new CustomTextBox();
        codeBox.parent = parent;
        codeBox.renderLayer = 5;
        codeBox.position = new UDim2(0.2f, 0.3f);
        codeBox.size = new UDim2(0.6f, 0.6f);
        codeBox.themeOverride.SetColour("buttonUpBackground", BearingColour.Transparent);
        codeBox.themeOverride.SetColour("buttonHoverBackground", BearingColour.Transparent);
        codeBox.themeOverride.SetColour("buttonDownBackground", BearingColour.Transparent);
        codeBox.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
        codeBox.themeOverride.SetColour("labelText", BearingColour.White);
        codeBox.useMultiline = true;
        codeBox.useSubtleSelection = true;
        codeBox.horizontalAlignment = HorizontalAlignment.Left;
        codeBox.verticalAlignment = VerticalAlignment.Top;
        gameObject.AddComponent(codeBox);

        UIManager.Sort();
    }
    public override void OnTick(float dt) {}
    public override void Cleanup() {}
}