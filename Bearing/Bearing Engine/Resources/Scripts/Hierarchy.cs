using OpenTK.Mathematics;
using Bearing;

public class Hierarchy : Component
{
	public static Hierarchy? instance;

	public GameObject? selectedObject;
	public UIButton? selectedButton;

	public event Action<GameObject> onHierarchyObjectSelected = (i) => {};

	private CustomPanel hierarchyPanel;
	private UIVerticalScrollView scroll;

	private int parent;
	public Hierarchy(int parent) {this.parent=parent; instance = this;}
    public override void OnLoad()
    {
    	hierarchyPanel = new CustomPanel("hierarchyPanel");
    	hierarchyPanel.theme = UIManager.themes["Objects"];
    	hierarchyPanel.renderLayer = 1;
    	hierarchyPanel.parent = parent;
    	hierarchyPanel.size = new UDim2(0.2f, 0.7f);

    	gameObject.AddComponent(hierarchyPanel);

    	scroll = new UIVerticalScrollView();
    	scroll.renderLayer = 2;
    	scroll.parent = hierarchyPanel.rid;
    	scroll.position = new UDim2(0,0,10,10);
    	scroll.size = new UDim2(1,0.7f, -20, -20);

    	gameObject.AddComponent(scroll);

    	UpdateHierarchy();
    }

    private CustomButton CreateTextButton(string text)
    {
    	CustomButton button = new CustomButton();
    	button.theme = UIManager.themes["Objects"];
    	button.size = new UDim2(1f, 0, 0, 40);
		button.renderLayer = 3;
		button.borderWidth = 4;
		button.visible = true;
		gameObject.AddComponent(button);

		UILabel label = new UILabel();
		label.theme = UIManager.themes["Objects"];
		label.parent = button.rid;
		label.renderLayer = 4;
		label.position = new UDim2(0,0,8,8);
		label.size = new UDim2(1,1,-16,-16);
		label.text = text;
		label.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
		label.useParentVisibility = true;
		gameObject.AddComponent(label);

		button.AddMeta(label);

		return button;
    }

    public void UpdateHierarchy()
    {
    	scroll.ClearContents();

    	CreateObjectButton((GameObject)Game.instance.root, 0);
    	CreateTreeRecursive(Game.instance.root, 1);

    	UIManager.Sort();
    }

    // returns true if created
    public bool CreateObjectButton(GameObject go, int indent)
    {
    	if (go.GetMeta<string>() == "___HIDEFROMHIERARCHY___")
			return false;

		CustomButton button = CreateTextButton(go.name);
		button.metadata = new object[] { go, button.metadata[0] };
		button.buttonPressed += HierarchySelectPressed;
		button.position = new UDim2(0,0,indent * 10,0);
		button.size = new UDim2(1,0,-indent * 10,40);

		if (selectedObject == go)
		{
    		selectedButton = button;
    		selectedButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(88,132,220));
		}

		if (go != Game.instance.root)
		{
			button.GetMeta<UILabel>(1).size = new UDim2(1,1,-16 - 20,-16);

			CustomButton removeButton = new CustomButton();
			removeButton.theme = UIManager.themes["Objects"];
	        removeButton.renderLayer = 4;
	        removeButton.parent = button.rid;
	        removeButton.anchor = new Vector2(1,0);
	        removeButton.position = new UDim2(1, 0, -2, 2);
	        removeButton.size = new UDim2(0, 1, 25, -4);
	        removeButton.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
	        removeButton.buttonPressed += (b) => {
	        	go.Cleanup();
	        	scroll.RemoveElement(button);
	        };
	        gameObject.AddComponent(removeButton);

	        UILabel removeLabel = new UILabel();
	        removeLabel.theme = UIManager.themes["Objects"];
	        removeLabel.renderLayer = 5;
	        removeLabel.parent = removeButton.rid;
	        removeLabel.position = new UDim2(0f,0f,4,4);
	        removeLabel.size = new UDim2(1f,1f,-8,-8);
	        removeLabel.text = "X";
	        removeLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
	        gameObject.AddComponent(removeLabel);
		}

		scroll.AddElement(button);

		return true;
    }

    public void CreateTreeRecursive(GameObject currentObject, int indent)
    {
    	foreach (GameObject go in currentObject.immediateChildren)
    	{
    		if (CreateObjectButton(go, indent))
    			CreateTreeRecursive(go, indent+1);
    	}
    }

    private void HierarchySelectPressed(UIButton sender)
    {
    	GameObject? go = sender.GetMeta<GameObject>();

    	if (go is null)
    		return;

    	if (go == selectedObject)
    	{
    		selectedObject = null;
    		selectedButton?.themeOverride.RemoveColour("panelOutline");
    	}
    	else
    	{
    		selectedObject = go;
    		selectedButton?.themeOverride.RemoveColour("panelOutline");
    		selectedButton = sender;
    		sender.themeOverride.SetColour("panelOutline", UIManager.themes["Objects"].GetColour("selection"));
    	}

    	if (selectedObject is not null)
    		onHierarchyObjectSelected.Invoke(selectedObject);

    	ComponentView.instance.UpdateComponentView();
    }

    public override void OnTick(float dt) {}
    public override void Cleanup() {}
}