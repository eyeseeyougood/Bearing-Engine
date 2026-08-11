using System.Reflection;
using OpenTK.Mathematics;
using Bearing;

public class ComponentView : Component
{
	public static ComponentView instance;

	private int parent;

	private CustomPanel componentViewPanel;
	private UIVerticalScrollView scroll;
	private CustomTextBox createCompMenu;

	private List<(PropertyInfo, object, bool)> waitingForSelection = new List<(PropertyInfo, object, bool)>();

	public ComponentView(int parent) {this.parent = parent; instance = this;}

    public override void OnLoad()
    {
    	componentViewPanel = new CustomPanel("componentViewPanel");
    	componentViewPanel.renderLayer = 1;
    	componentViewPanel.parent = parent;
    	componentViewPanel.position = new UDim2(0.8f, 0f);
    	componentViewPanel.size = new UDim2(0.2f, 1f);
    	gameObject.AddComponent(componentViewPanel);

    	scroll = new UIVerticalScrollView();
    	scroll.renderLayer = 2;
    	scroll.parent = componentViewPanel.rid;
    	scroll.position = new UDim2(0,0,10,10);
    	scroll.size = new UDim2(1,1, -20, -100);
    	scroll.themeOverride.SetColour("verticalScrollBG", BearingColour.Transparent);
    	gameObject.AddComponent(scroll);

    	CustomButton createCompButton = new CustomButton();
        createCompButton.renderLayer = 2;
        createCompButton.parent = componentViewPanel.rid;
        createCompButton.anchor = new Vector2(0,1);
        createCompButton.position = new UDim2(0, 1, 10, -10);
        createCompButton.size = new UDim2(1, 0, -20, 70);
        createCompButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        createCompButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        createCompButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
        createCompButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(228,182,183));
        createCompButton.buttonPressed += ToggleCreateCompMenu;
        gameObject.AddComponent(createCompButton);

        UILabel createCompLabel = new UILabel();
        createCompLabel.renderLayer = 3;
        createCompLabel.parent = createCompButton.rid;
        createCompLabel.position = new UDim2(0.05f,0.05f,8,8);
        createCompLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        createCompLabel.text = "Add Component";
        createCompLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
        createCompLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(createCompLabel);

        CreateCreateCompMenu();

        // link selection
        Hierarchy.instance.onHierarchyObjectSelected += HierarchySelected;
    }

    public UIPanel? GetComponentPanel(Type componentType)
    {
    	foreach (UIElement element in scroll.GetContents())
    	{
    		if ((Type)element.metadata[1] == componentType)
    		{
    			return (UIPanel)element;
    		}
    	}

    	return null;
    }

    private void HierarchySelected(GameObject selected)
    {
    	foreach ((PropertyInfo, object, bool) v in waitingForSelection)
    	{
    		v.Item1.SetValue(v.Item2, selected);

    		if (v.Item3) // should refresh hierarchy bool
    			Hierarchy.instance.UpdateHierarchy();
    	}

    	waitingForSelection.Clear();
    }

    private void CreateCreateCompMenu()
    {
    	createCompMenu = new CustomTextBox();
    	createCompMenu.renderLayer = 100;
    	createCompMenu.parent = parent;
    	createCompMenu.anchor = new Vector2(0.5f, 0.5f);
    	createCompMenu.position = new UDim2(0.5f, 0.5f);
    	createCompMenu.size = new UDim2(0.4f, 0f, 0, 100);
    	createCompMenu.useParentVisibility = false;
    	createCompMenu.useParentActivity = false;
    	createCompMenu.visible = false;
    	createCompMenu.active = false;
    	createCompMenu.textSubmitted += CreateCompSubmit;
    	gameObject.AddComponent(createCompMenu);
    }

    private void CreateCompSubmit(UITextBox textbox)
    {
    	textbox.visible = false;
    	textbox.active = false;

    	if (Hierarchy.instance?.selectedObject is null)
    		return;

    	try{
	    	string typeName = textbox.text;
	    	Component newComponent = (Component)Activator.CreateInstance(Type.GetType(typeName));

	    	Hierarchy.instance.selectedObject.AddComponent(newComponent);

    		CreateComponentPanel(newComponent);
    		UIManager.Sort();
    	}catch(Exception e) {Logger.LogError("Failed to create component: " + e.Message);}
    }

    private void ToggleCreateCompMenu(UIButton sender)
    {
    	createCompMenu.visible = !createCompMenu.visible;
    	createCompMenu.active = !createCompMenu.active;
    }

    private CustomTextBox CreateInputField(int parent, string value)
    {
    	CustomTextBox inp = new CustomTextBox("input field");
    	inp.parent = parent;
    	inp.renderLayer = 6;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.166667f,1,-1,0);
		inp.borderWidth = 2;
		inp.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        inp.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        inp.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
		inp.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		inp.placeholderText = value;
		inp.textSubmitted += (t) => {
			t.placeholderText = t.text;
			t.text = "";
		};
		inp.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;

    	gameObject.AddComponent(inp);

    	return inp;
    }

    public void CreateComponentPanel(object c)
    {
    	CustomPanel panel = new CustomPanel("Component Title", c.GetType());
    	panel.renderLayer = 3;
    	panel.size = new UDim2(1,0,-20,300);
    	panel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(212,125,199));
    	gameObject.AddComponent(panel);

    	UILabel name = new UILabel();
    	name.renderLayer = 4;
    	name.parent = panel.rid;
    	name.position = new UDim2(0,0,10,10);
    	name.size = new UDim2(1f,0,-40,35);
    	name.text = c.GetType().Name;
    	gameObject.AddComponent(name);

    	if (c.GetType().IsSubclassOf(typeof(Component)))
    	{
	    	CustomButton removeButton = new CustomButton();
	        removeButton.renderLayer = 4;
	        removeButton.parent = panel.rid;
	        removeButton.anchor = new Vector2(1,0);
	        removeButton.position = new UDim2(1, 0, -12, 10);
	        removeButton.size = new UDim2(0, 0, 25, 35);
	        removeButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
	        removeButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
	        removeButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
	        removeButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(0,0,0,0));
	        removeButton.buttonPressed += (b) => {
	        	((Component)c).gameObject.RemoveComponent((Component)c);
	        	scroll.RemoveElement(panel);
	        	panel.Cleanup();
	        };
	        gameObject.AddComponent(removeButton);

	        UILabel removeLabel = new UILabel();
	        removeLabel.renderLayer = 5;
	        removeLabel.parent = removeButton.rid;
	        removeLabel.position = new UDim2(0f,0f,4,4);
	        removeLabel.size = new UDim2(1f,1f,-8,-8);
	        removeLabel.text = "X";
	        removeLabel.themeOverride.SetColour("labelText", BearingColour.FromZeroTo255(213, 156, 205));
	        removeLabel.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
	        gameObject.AddComponent(removeLabel);
    	}

    	UIVerticalScrollView panelScroll = new UIVerticalScrollView();
    	panelScroll.renderLayer = 4;
    	panelScroll.parent = panel.rid;
    	panelScroll.position = new UDim2(0,0,3,50);
    	panelScroll.size = new UDim2(1,1, -6, -60);
    	panelScroll.themeOverride.SetColour("verticalScrollBG", BearingColour.Transparent);
    	gameObject.AddComponent(panelScroll);

    	panel.AddMeta(panelScroll);

    	int index = 0;
    	foreach (PropertyInfo p in c.GetType().GetProperties())
    	{
    		object? value = p.GetValue(c);

    		if (p.GetCustomAttribute(typeof(HideFromInspectorAttribute)) is not null)
    			continue;

    		bool doRefreshHierarchy = new List<string>() {
    		
    			"GameObject.name", "GameObject.parent"
			
			}.Contains(p.DeclaringType.Name + "." + p.Name);

    		CustomPanel propertyPanel;

    		if (value is not null)
    		{
	    		switch (value.GetType().Name)
	    		{
	    			case "Vector3":
	    				propertyPanel = CreateVector3(p, c, (Vector3)value, refreshHierarchy: doRefreshHierarchy);
	    				break;
	    			case "Boolean":
	    				propertyPanel = CreateBool(p, c, (bool)value, refreshHierarchy: doRefreshHierarchy);
	    				break;
	    			case "Int32":
	    				propertyPanel = CreateInt32(p, c, (int)value, refreshHierarchy: doRefreshHierarchy);
	    				break;
	    			case "Single":
	    				propertyPanel = CreateFloat(p, c, (float)value, refreshHierarchy: doRefreshHierarchy);
	    				break;
	    			case "GameObject":
	    				propertyPanel = CreateObjectSelection(p, c, (GameObject)value, refreshHierarchy: doRefreshHierarchy);
	    				break;
					default:
	    				propertyPanel = CreateString(p, c, value.ToString(), refreshHierarchy: doRefreshHierarchy);
	    				break;
	    		}
    		}
    		else
    			propertyPanel = CreateString(p, c, "NULL");

    		panelScroll.AddElement(propertyPanel);

    		index++;
    	}

    	scroll.AddElement(panel);
    }

    private CustomPanel CreateBool(PropertyInfo prop, object component, bool value, bool refreshHierarchy = false)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel("Bool panel");
    	panel.renderLayer = 5;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(0,0,0));
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.renderLayer = 7;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomButton toggle = new CustomButton("toggle field");
    	toggle.parent = panel.rid;
    	toggle.renderLayer = 6;
    	toggle.position = new UDim2(0.5f,0,0,0);
    	toggle.size = new UDim2(0.5f,1,-4,0);
		toggle.borderWidth = 2;
		toggle.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        toggle.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        toggle.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
		toggle.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		toggle.buttonPressed += (b) => {
			bool v = b.GetMeta<UILabel>(2)?.text == "True";
			v = !v;
			b.GetMeta<UILabel>(2).text = v.ToString();

			b.GetMeta<PropertyInfo>().SetValue(b.GetMeta<object>(1), v);
		};
		toggle.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(toggle);

    	UILabel toggleText = new UILabel();
    	toggleText.renderLayer = 7;
    	toggleText.parent = toggle.rid;
    	toggleText.position = new UDim2(0,0,2,2);
    	toggleText.size = new UDim2(1,1,-4,-4);
    	toggleText.text = value.ToString();
    	toggleText.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(toggleText);

    	toggle.metadata = new object[] {prop, component, toggleText};

    	return panel;
    }

    private CustomPanel CreateVector3(PropertyInfo prop, object component, Vector3 value, bool refreshHierarchy = false)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(0,0,0));

    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;

    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.renderLayer = 7;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;

    	gameObject.AddComponent(name);

    	CustomTextBox x = CreateInputField(panel.rid, value.X.ToString());
    	x.renderLayer = 6;
    	x.position = new UDim2(0.5f,0,0,0);
    	x.size = new UDim2(0.166667f,1,-1,0);
    	x.truncateThreshold = 4; // perhaps values like this could be saved into an editor settings file or sum
    	x.useElipsisTruncation = true;
    	x.ResetTexture();
    	x.textSubmitted += (s) => {
    		if (float.TryParse(s.placeholderText, out float val))
    		{
    			Vector3 curr = (Vector3)x.GetMeta<PropertyInfo>()?.GetValue(x.GetMeta<object>(1));
    			x.GetMeta<PropertyInfo>()?.SetValue(x.GetMeta<object>(1), new Vector3(val, curr.Y, curr.Z));
    			x.metadata[2] = val;
    		}
    		else
    		{
    			x.placeholderText = ((float)x.metadata[2]).ToString();
    		}

    		if (refreshHierarchy)
    			Hierarchy.instance?.UpdateHierarchy();

			x.truncateThreshold = 4;
    	};
    	x.buttonPressed += (b) => {
			x.truncateThreshold = -1;
    	};
    	x.metadata = new object[] { prop, component, value.X };

    	CustomTextBox y = CreateInputField(panel.rid, value.Y.ToString());
    	y.renderLayer = 6;
    	y.position = new UDim2(0.5f + 0.166667f,0,0,0);
    	y.size = new UDim2(0.166667f,1,-1,0);
    	y.truncateThreshold = 4; // perhaps values like this could be saved into an editor settings file or sum
    	y.useElipsisTruncation = true;
    	y.ResetTexture();
    	y.textSubmitted += (s) => {
    		if (float.TryParse(s.placeholderText, out float val))
    		{
    			Vector3 curr = (Vector3)y.GetMeta<PropertyInfo>()?.GetValue(y.GetMeta<object>(1));
    			y.GetMeta<PropertyInfo>()?.SetValue(y.GetMeta<object>(1), new Vector3(curr.X, val, curr.Z));
    			y.metadata[2] = val;
    		}
    		else
    		{
    			y.placeholderText = ((float)y.metadata[2]).ToString();
    		}

    		if (refreshHierarchy)
    			Hierarchy.instance?.UpdateHierarchy();

			y.truncateThreshold = 4;
    	};
    	y.buttonPressed += (b) => {
			y.truncateThreshold = -1;
    	};
    	y.metadata = new object[] { prop, component, value.Y };

		CustomTextBox z = CreateInputField(panel.rid, value.Z.ToString());
    	z.renderLayer = 6;
    	z.position = new UDim2(0.5f + 2 * 0.166667f,0,0,0);
    	z.size = new UDim2(0.166667f,1,-1,0);
    	z.truncateThreshold = 4; // perhaps values like this could be saved into an editor settings file or sum
    	z.useElipsisTruncation = true;
    	z.ResetTexture();
    	z.textSubmitted += (s) => {
    		if (float.TryParse(s.placeholderText, out float val))
    		{
    			Vector3 curr = (Vector3)z.GetMeta<PropertyInfo>()?.GetValue(z.GetMeta<object>(1));
    			z.GetMeta<PropertyInfo>()?.SetValue(z.GetMeta<object>(1), new Vector3(curr.X, curr.Y, val));
    			z.metadata[2] = val;
    		}
    		else
    		{
    			z.placeholderText = ((float)z.metadata[2]).ToString();
    		}

    		if (refreshHierarchy)
    			Hierarchy.instance?.UpdateHierarchy();

			z.truncateThreshold = 4;
    	};
    	z.buttonPressed += (b) => {
			z.truncateThreshold = -1;
    	};
    	z.metadata = new object[] { prop, component, value.Z };

    	panel.AddMeta(x,y,z);

    	return panel;
    }

    private CustomPanel CreateString(PropertyInfo prop, object component, string value, bool refreshHierarchy = false)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(0,0,0));
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.renderLayer = 7;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomTextBox inp = CreateInputField(panel.rid, value);
    	inp.renderLayer = 6;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.5f,1,-4,0);
    	inp.textSubmitted += (s) => {
    		inp.GetMeta<PropertyInfo>()?.SetValue(inp.GetMeta<object>(1), s.placeholderText);

    		if (refreshHierarchy)
    			Hierarchy.instance?.UpdateHierarchy();
    	};
    	inp.metadata = new object[] { prop, component };

    	return panel;
    }

	private CustomPanel CreateFloat(PropertyInfo prop, object component, float value, bool refreshHierarchy = false)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(0,0,0));
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.renderLayer = 7;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomTextBox inp = CreateInputField(panel.rid, value.ToString());
    	inp.renderLayer = 6;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.5f,1,-4,0);
    	inp.textSubmitted += (s) => {
    		if (float.TryParse(s.placeholderText, out float val))
    		{
    			inp.GetMeta<PropertyInfo>()?.SetValue(inp.GetMeta<object>(1), val);
    			inp.metadata[2] = val;
    		}
    		else
    		{
    			inp.placeholderText = ((float)inp.metadata[2]).ToString();
    		}

    		if (refreshHierarchy)
    			Hierarchy.instance?.UpdateHierarchy();
    	};
    	inp.metadata = new object[] { prop, component, value };

    	return panel;
    }

    private CustomPanel CreateInt32(PropertyInfo prop, object component, int value, bool refreshHierarchy = false)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(0,0,0));
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.renderLayer = 7;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomTextBox inp = CreateInputField(panel.rid, value.ToString());
    	inp.renderLayer = 6;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.5f,1,-4,0);
    	inp.textSubmitted += (s) => {
    		if (int.TryParse(s.placeholderText, out int val))
    		{
    			inp.GetMeta<PropertyInfo>()?.SetValue(inp.GetMeta<object>(1), val);
    			inp.metadata[2] = val;
    		}
    		else
    		{
    			inp.placeholderText = ((int)inp.metadata[2]).ToString();
    		}

    		if (refreshHierarchy)
    			Hierarchy.instance?.UpdateHierarchy();
    	};
    	inp.metadata = new object[] { prop, component, value };

    	return panel;
    }

    private CustomPanel CreateObjectSelection(PropertyInfo prop, object component, GameObject value, bool refreshHierarchy = false)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(0,0,0));
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.renderLayer = 7;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomButton valButton = new CustomButton();
    	valButton.parent = panel.rid;
    	valButton.renderLayer = 6;
    	valButton.position = new UDim2(0.5f,0,0,0);
    	valButton.size = new UDim2(0.5f,1,-4,0);
		valButton.borderWidth = 2;
		valButton.themeOverride.SetColour("buttonUpBackground", BearingColour.FromZeroTo255(40,40,40));
        valButton.themeOverride.SetColour("buttonDownBackground", BearingColour.FromZeroTo255(30,30,30));
        valButton.themeOverride.SetColour("buttonHoverBackground", BearingColour.FromZeroTo255(55,55,55));
		valButton.themeOverride.SetColour("panelOutline", BearingColour.FromZeroTo255(245,149,169));
		valButton.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
		valButton.buttonPressed += (b) => {
			PropertyInfo p = b.GetMeta<PropertyInfo>();
			object c = b.GetMeta<Object>(1);
			(PropertyInfo, object, bool) v = (p, c, refreshHierarchy);
			if (waitingForSelection.Contains(v))
			{
				waitingForSelection.Remove(v);
			}
			else
			{
				waitingForSelection.Add(v);
			}

			if (refreshHierarchy)
				Hierarchy.instance.UpdateHierarchy();
		};
    	gameObject.AddComponent(valButton);

    	UILabel valText = new UILabel();
    	valText.renderLayer = 7;
    	valText.parent = valButton.rid;
    	valText.position = new UDim2(0,0,2,2);
    	valText.size = new UDim2(1,1,-4,-4);
    	valText.text = value.name;
    	valText.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(valText);

    	valButton.metadata = new object[] { prop, component, valText };
/*
    	Custom inp = CreateInputField(panel.rid, value.name);
    	inp.renderLayer = 6;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.5f,1,-4,0);
    	inp.textSubmitted += (s) => {
    		inp.GetMeta<PropertyInfo>()?.SetValue(inp.GetMeta<object>(1), s.placeholderText);

    		if (refreshHierarchy)
    			Hierarchy.instance?.UpdateHierarchy();
    	};
    	inp.metadata = new object[] { prop, component };*/

    	return panel;
    }

    public void UpdateComponentView()
    {
    	scroll.ClearContents();

    	if (Hierarchy.instance.selectedObject is null)
    		return;

		CreateComponentPanel(Hierarchy.instance.selectedObject);
		CreateComponentPanel(Hierarchy.instance.selectedObject.transform);
    	foreach (Component c in Hierarchy.instance.selectedObject.components)
		{
			CreateComponentPanel(c);
		}

		UIManager.Sort();
    }

    public override void OnTick(float dt) {}
    public override void Cleanup() {}
}