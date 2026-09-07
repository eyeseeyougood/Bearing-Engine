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
    	componentViewPanel.theme = UIManager.themes["Objects"];
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
    	gameObject.AddComponent(scroll);

    	CustomButton createCompButton = new CustomButton();
    	createCompButton.theme = UIManager.themes["BigButtons"];
        createCompButton.renderLayer = 2;
        createCompButton.parent = componentViewPanel.rid;
        createCompButton.anchor = new Vector2(0,1);
        createCompButton.position = new UDim2(0, 1, 10, -10);
        createCompButton.size = new UDim2(1, 0, -20, 70);
        createCompButton.buttonPressed += ToggleCreateCompMenu;
        gameObject.AddComponent(createCompButton);

        UILabel createCompLabel = new UILabel();
        createCompLabel.theme = UIManager.themes["BigButtons"];
        createCompLabel.renderLayer = 3;
        createCompLabel.parent = createCompButton.rid;
        createCompLabel.position = new UDim2(0.05f,0.05f,8,8);
        createCompLabel.size = new UDim2(0.9f,0.9f,-16,-16);
        createCompLabel.text = "Add Component";
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
    	inp.theme = UIManager.themes["ListItems"];
    	inp.parent = parent;
    	inp.renderLayer = 6;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.166667f,1,-1,0);
		inp.borderWidth = 2;
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
    	panel.theme = UIManager.themes["Objects"];
    	panel.renderLayer = 3;
    	panel.size = new UDim2(1,0,-20,300);
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
	    	removeButton.theme = UIManager.themes["Objects"];
	        removeButton.renderLayer = 4;
	        removeButton.parent = panel.rid;
	        removeButton.anchor = new Vector2(1,0);
	        removeButton.position = new UDim2(1, 0, -12, 10);
	        removeButton.size = new UDim2(0, 0, 25, 35);
	        removeButton.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
	        removeButton.buttonPressed += (b) => {
	        	((Component)c).gameObject.RemoveComponent((Component)c);
	        	scroll.RemoveElement(panel);
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

    	UIVerticalScrollView panelScroll = new UIVerticalScrollView();
    	panelScroll.renderLayer = 4;
    	panelScroll.parent = panel.rid;
    	panelScroll.position = new UDim2(0,0,3,50);
    	panelScroll.size = new UDim2(1,1, -6, -60);
    	gameObject.AddComponent(panelScroll);

    	panel.AddMeta(panelScroll);

    	FillScrollWithProperties(panelScroll, c);

    	scroll.AddElement(panel);
    }

    public void HandleCustomInspectorSetProperty(object target, PropertyInfo property)
    {
    	MethodInfo? method = target.GetType().GetMethod("OnPropertyChanged");

    	if (method is null)
    		return;

    	if (method.GetCustomAttribute<CustomInspectorMethodAttribute>() is null)
    		return;

    	method.Invoke(target, new object[] { property });
    }

    private CustomPanel CreateBool(PropertyInfo prop, object component, bool? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel("Bool panel");
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7 + renderLayerOffset;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomButton toggle = new CustomButton("toggle field");
    	toggle.theme = UIManager.themes["ListItems"];
    	toggle.parent = panel.rid;
    	toggle.renderLayer = 6 + renderLayerOffset;
    	toggle.position = new UDim2(0.5f,0,0,0);
    	toggle.size = new UDim2(0.5f,1,-4,0);
		toggle.borderWidth = 2;
		toggle.buttonPressed += (b) => {
			bool v = b.GetMeta<UILabel>(2)?.text == "True";
			v = !v;
			b.GetMeta<UILabel>(2).text = v.ToString();

			b.GetMeta<PropertyInfo>().SetValue(b.GetMeta<object>(1), v);
			HandleCustomInspectorSetProperty(b.GetMeta<object>(1), b.GetMeta<PropertyInfo>());
		};
		toggle.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(toggle);

    	UILabel toggleText = new UILabel();
    	toggleText.renderLayer = 7 + renderLayerOffset;
    	toggleText.parent = toggle.rid;
    	toggleText.position = new UDim2(0,0,2,2);
    	toggleText.size = new UDim2(1,1,-4,-4);
    	toggleText.text = value is null ? "NULL" : value.ToString();
    	toggleText.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
    	gameObject.AddComponent(toggleText);

    	toggle.metadata = new object[] {prop, component, toggleText};

    	return panel;
    }

    private void FillScrollWithProperties(UIVerticalScrollView scroll, object objectWithProperties, int renderLayerOffset = 0)
    {
    	foreach (PropertyInfo prop in objectWithProperties.GetType().GetProperties())
    	{
    		if (!prop.CanWrite)
    			continue;

    		object? value = null;
    		try{
	    		value = prop.GetValue(objectWithProperties);
    		}
    		catch{ // value shouldn't be fetched, so skip it
    			continue;
    		}

    		if (prop.GetCustomAttribute(typeof(HideFromInspectorAttribute)) is not null)
    			continue;

    		bool doRefreshHierarchy = new List<string>() {
    		
    			"GameObject.name", "GameObject.parent"
			
			}.Contains(prop.DeclaringType?.Name + "." + prop.Name);

    		CustomPanel propertyPanel;

			Logger.Log("type: " + prop.PropertyType.Name);
    		switch (prop.PropertyType.Name)
    		{
    			case "Vector4":
    				propertyPanel = CreateVector4(prop, objectWithProperties, (Vector4?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    			case "Vector3":
    				propertyPanel = CreateVector3(prop, objectWithProperties, (Vector3?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    			case "Boolean":
    				propertyPanel = CreateBool(prop, objectWithProperties, (bool?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    			case "Int32":
    				propertyPanel = CreateInt32(prop, objectWithProperties, (int?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    			case "Single":
    				propertyPanel = CreateFloat(prop, objectWithProperties, (float?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    			case "Resource":
    				propertyPanel = CreateResource(prop, objectWithProperties, (Resource?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    			case "EmbeddedResource":
    				propertyPanel = CreateResource(prop, objectWithProperties, (Resource?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    			case "GameObject":
    				propertyPanel = CreateObjectSelection(prop, objectWithProperties, (GameObject?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    			case "String":
    				propertyPanel = CreateString(prop, objectWithProperties, (string?)value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
				default:
    				propertyPanel = CreateComplex(prop, objectWithProperties, value, refreshHierarchy: doRefreshHierarchy, renderLayerOffset);
    				break;
    		}

    		scroll.AddElement(propertyPanel);
    	}
    }

    private Dictionary<UIPanel, UIPanel> dropdownTree = new Dictionary<UIPanel, UIPanel>();
    private CustomPanel? CreateDropdown(UDim2 pos, int renderLayer, object? objectWithProperties, UIPanel parentPanel)
    {
    	if (objectWithProperties is null)
    		return null;

    	CustomPanel dropdownPanel = new CustomPanel();
    	dropdownPanel.theme = UIManager.themes["Objects"];
    	dropdownPanel.parent = parent;
    	dropdownPanel.renderLayer = renderLayer + 2;
    	dropdownPanel.anchor = new Vector2(1,0);
    	dropdownPanel.position = pos;
    	dropdownPanel.size = new UDim2(0,0,400,300);
    	dropdownPanel.borderWidth = 3;
    	gameObject.AddComponent(dropdownPanel);

    	UIVerticalScrollView scroll = new UIVerticalScrollView();
    	scroll.parent = dropdownPanel.rid;
    	scroll.renderLayer = renderLayer + 3;
    	scroll.position = new UDim2(0,0, 3, 10);
    	scroll.size = new UDim2(1,1, -6, -20);
    	gameObject.AddComponent(scroll);

    	FillScrollWithProperties(scroll, objectWithProperties, renderLayer);

    	// rescale panel
    	int itemCount = scroll.GetContents().Count;
    	float size = 50 * itemCount + 5;
    	size = Math.Clamp(size, 100, 50 * 6 + 5);
    	dropdownPanel.size = new UDim2(0,0,400,size);

    	// push panel up if too low 
    	float furthest = dropdownPanel.worldPosition.Normalized(Game.instance.ClientSize).Y +
			dropdownPanel.worldSize.Normalized(Game.instance.ClientSize).Y;

    	if (furthest > 1)
    	{
    		dropdownPanel.position -= new UDim2(0,furthest-1,190,0);
    	}

    	// register dropdown

    	RegisterDropdown(parentPanel, dropdownPanel);
    	dropdownPanel.AddMeta(scroll);

    	UIManager.Sort();

    	return dropdownPanel;
    }

    // Current dropdown metadata layout: [UIVERTSCROLL that it owns, UIButton which created the dropdown]
    private void RemoveDropdown(UIPanel dropdown)
    {
		dropdown.GetMeta<UIButton>(1)?.RemoveMeta(dropdown);

		if (dropdownTree.ContainsKey(dropdown))
		{
			RemoveDropdown(dropdownTree[dropdown]);
		}
		dropdownTree.Remove(dropdown);

		// cleanup current dropdown
		UIVerticalScrollView? scroll = dropdown.GetMeta<UIVerticalScrollView>();
		if (scroll is null)
		{
			throw new Exception("How, have, you, done, this>?>?>?//.>.?.>?>?");
		}
		foreach (UIElement elem in scroll.GetContents())
		{
			foreach (object meta in elem.metadata)
			{
				if (meta is UIButton button)
				{
					if (FileAssignableRegistry.IsButtonFileAssignable(button))
					{
						FileAssignableRegistry.UnregisterFileAssignableButton(button);
					}
				}
			}
		}
		gameObject.RemoveComponent(dropdown);
    }

    private void RegisterDropdown(UIPanel parentPanel, UIPanel childPanel)
    {
    	if (dropdownTree.ContainsKey(parentPanel))
    	{
    		RemoveDropdown(dropdownTree[parentPanel]);
    		dropdownTree.Remove(parentPanel);
    	}

		dropdownTree.Add(parentPanel, childPanel);
    }

    private CustomPanel CreateResource(PropertyInfo prop, object component, Resource? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel("resource panel");
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7 + renderLayerOffset;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomButton button = new CustomButton("resource field");
    	button.theme = UIManager.themes["ListItems"];
    	button.parent = panel.rid;
    	button.renderLayer = 6 + renderLayerOffset;
    	button.position = new UDim2(0.5f,0,0,0);
    	button.size = new UDim2(0.5f,1,-4,0);
		button.borderWidth = 2;
		button.buttonPressed += (b) => {
			CustomPanel? existingDropdown = button.GetMeta<CustomPanel>(3);
			if (existingDropdown is null)
			{
				UIPanel parentPanel = (UIPanel)UIManager.FindFromRID(UIManager.FindFromRID(panel.parent).parent);
				CustomPanel? dropdown = CreateDropdown(button.worldPosition + button.worldSize, button.renderLayer, value, parentPanel);
				if (dropdown is null)
					return;
				dropdown.AddMeta(button);
				button.AddMeta(dropdown);
			}
			else
			{
				RemoveDropdown(existingDropdown);
			}
		};
		button.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(button);

    	UILabel buttonText = new UILabel();
    	buttonText.theme = UIManager.themes["ListItems"];
    	buttonText.renderLayer = 7 + renderLayerOffset;
    	buttonText.parent = button.rid;
    	buttonText.position = new UDim2(0,0,2,2);
    	buttonText.size = new UDim2(1,1,-4,-4);
    	buttonText.text = value is null ? "NULL" : value.ToString();
    	buttonText.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
    	gameObject.AddComponent(buttonText);

    	// register button as file assignable
    	FileAssignableRegistry.RegisterFileAssignableButton(button);
    	panel.AddMeta(button); // THIS IS HORRIBLE JANK

    	button.metadata = new object[] {prop, component, buttonText};

    	return panel;
    }

    private CustomPanel CreateComplex(PropertyInfo prop, object component, object? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel("complex panel");
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7 + renderLayerOffset;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomButton button = new CustomButton("complex field");
    	button.theme = UIManager.themes["ListItems"];
    	button.parent = panel.rid;
    	button.renderLayer = 6 + renderLayerOffset;
    	button.position = new UDim2(0.5f,0,0,0);
    	button.size = new UDim2(0.5f,1,-4,0);
		button.borderWidth = 2;
		button.buttonPressed += (b) => {
			CustomPanel? existingDropdown = button.GetMeta<CustomPanel>(3);
			if (existingDropdown is null)
			{
				UIPanel parentPanel = (UIPanel)UIManager.FindFromRID(UIManager.FindFromRID(panel.parent).parent);
				CustomPanel? dropdown = CreateDropdown(button.worldPosition + button.worldSize, button.renderLayer, value, parentPanel);
				if (dropdown is null)
					return;
				dropdown.AddMeta(button);
				button.AddMeta(dropdown);
			}
			else
			{
				RemoveDropdown(existingDropdown);
			}
		};
		button.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(button);

    	UILabel buttonText = new UILabel();
    	buttonText.theme = UIManager.themes["ListItems"];
    	buttonText.renderLayer = 7 + renderLayerOffset;
    	buttonText.parent = button.rid;
    	buttonText.position = new UDim2(0,0,2,2);
    	buttonText.size = new UDim2(1,1,-4,-4);
    	buttonText.text = value is null ? "NULL" : value.ToString();
    	buttonText.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
    	gameObject.AddComponent(buttonText);

    	button.metadata = new object[] {prop, component, buttonText};

    	return panel;
    }

    private CustomPanel CreateVector3(PropertyInfo prop, object component, Vector3? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;

    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7 + renderLayerOffset;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;

    	gameObject.AddComponent(name);

    	CustomTextBox x = CreateInputField(panel.rid, value.HasValue ? value.Value.X.ToString() : "ERR");
    	x.renderLayer = 6 + renderLayerOffset;
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
    			HandleCustomInspectorSetProperty(x.GetMeta<object>(1), x.GetMeta<PropertyInfo>());
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
    	x.metadata = new object[] { prop, component, value.Value.X };

    	CustomTextBox y = CreateInputField(panel.rid, value.HasValue ? value.Value.Y.ToString() : "ERR");
    	y.renderLayer = 6 + renderLayerOffset;
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
    			HandleCustomInspectorSetProperty(y.GetMeta<object>(1), y.GetMeta<PropertyInfo>());
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
    	y.metadata = new object[] { prop, component, value.Value.Y };

		CustomTextBox z = CreateInputField(panel.rid, value.HasValue ? value.Value.Z.ToString() : "ERR");
    	z.renderLayer = 6 + renderLayerOffset;
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
    			HandleCustomInspectorSetProperty(z.GetMeta<object>(1), z.GetMeta<PropertyInfo>());
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
    	z.metadata = new object[] { prop, component, value.Value.Z };

    	panel.AddMeta(x,y,z);

    	return panel;
    }

	private CustomPanel CreateVector4(PropertyInfo prop, object component, Vector4? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;

    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7 + renderLayerOffset;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;

    	gameObject.AddComponent(name);

    	CustomTextBox x = CreateInputField(panel.rid, value.HasValue ? value.Value.X.ToString() : "ERR");
    	x.renderLayer = 6 + renderLayerOffset;
    	x.position = new UDim2(0.5f,0,0,0);
    	x.size = new UDim2(0.125f,1,-1,0);
    	x.truncateThreshold = 4; // perhaps values like this could be saved into an editor settings file or sum
    	x.useElipsisTruncation = true;
    	x.ResetTexture();
    	x.textSubmitted += (s) => {
    		if (float.TryParse(s.placeholderText, out float val))
    		{
    			Vector4 curr = (Vector4)x.GetMeta<PropertyInfo>()?.GetValue(x.GetMeta<object>(1));
    			x.GetMeta<PropertyInfo>()?.SetValue(x.GetMeta<object>(1), new Vector4(val, curr.Y, curr.Z, curr.W));
    			HandleCustomInspectorSetProperty(x.GetMeta<object>(1), x.GetMeta<PropertyInfo>());
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
    	x.metadata = new object[] { prop, component, value.Value.X };

    	CustomTextBox y = CreateInputField(panel.rid, value.HasValue ? value.Value.Y.ToString() : "ERR");
    	y.renderLayer = 6 + renderLayerOffset;
    	y.position = new UDim2(0.5f + 0.125f,0,0,0);
    	y.size = new UDim2(0.125f,1,-1,0);
    	y.truncateThreshold = 4; // perhaps values like this could be saved into an editor settings file or sum
    	y.useElipsisTruncation = true;
    	y.ResetTexture();
    	y.textSubmitted += (s) => {
    		if (float.TryParse(s.placeholderText, out float val))
    		{
    			Vector4 curr = (Vector4)y.GetMeta<PropertyInfo>()?.GetValue(y.GetMeta<object>(1));
    			y.GetMeta<PropertyInfo>()?.SetValue(y.GetMeta<object>(1), new Vector4(curr.X, val, curr.Z, curr.W));
    			HandleCustomInspectorSetProperty(y.GetMeta<object>(1), y.GetMeta<PropertyInfo>());
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
    	y.metadata = new object[] { prop, component, value.Value.Y };

		CustomTextBox z = CreateInputField(panel.rid, value.HasValue ? value.Value.Z.ToString() : "ERR");
    	z.renderLayer = 6 + renderLayerOffset;
    	z.position = new UDim2(0.5f + 2 * 0.125f,0,0,0);
    	z.size = new UDim2(0.125f,1,-1,0);
    	z.truncateThreshold = 4; // perhaps values like this could be saved into an editor settings file or sum
    	z.useElipsisTruncation = true;
    	z.ResetTexture();
    	z.textSubmitted += (s) => {
    		if (float.TryParse(s.placeholderText, out float val))
    		{
    			Vector4 curr = (Vector4)z.GetMeta<PropertyInfo>()?.GetValue(z.GetMeta<object>(1));
    			z.GetMeta<PropertyInfo>()?.SetValue(z.GetMeta<object>(1), new Vector4(curr.X, curr.Y, val, curr.W));
    			HandleCustomInspectorSetProperty(z.GetMeta<object>(1), z.GetMeta<PropertyInfo>());
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
    	z.metadata = new object[] { prop, component, value.Value.Z };

    	panel.AddMeta(x,y,z);

    	return panel;
    }

    private CustomPanel CreateString(PropertyInfo prop, object component, string? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7 + renderLayerOffset;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomTextBox inp = CreateInputField(panel.rid, value is null ? "NULL" : value);
    	inp.renderLayer = 6 + renderLayerOffset;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.5f,1,-4,0);
    	inp.textSubmitted += (s) => {
    		inp.GetMeta<PropertyInfo>()?.SetValue(inp.GetMeta<object>(1), s.placeholderText);
    		HandleCustomInspectorSetProperty(inp.GetMeta<object>(1), inp.GetMeta<PropertyInfo>());

    		if (refreshHierarchy)
    			Hierarchy.instance?.UpdateHierarchy();
    	};
    	inp.metadata = new object[] { prop, component };

    	return panel;
    }

	private CustomPanel CreateFloat(PropertyInfo prop, object component, float? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomTextBox inp = CreateInputField(panel.rid, value is null ? "NULL" : value.ToString());
    	inp.renderLayer = 6 + renderLayerOffset;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.5f,1,-4,0);
    	inp.textSubmitted += (s) => {
    		if (float.TryParse(s.placeholderText, out float val))
    		{
    			inp.GetMeta<PropertyInfo>()?.SetValue(inp.GetMeta<object>(1), val);
    			HandleCustomInspectorSetProperty(inp.GetMeta<object>(1), inp.GetMeta<PropertyInfo>());
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

    private CustomPanel CreateInt32(PropertyInfo prop, object component, int? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7 + renderLayerOffset;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomTextBox inp = CreateInputField(panel.rid, value is null ? "NULL" : value.ToString());
    	inp.renderLayer = 6 + renderLayerOffset;
    	inp.position = new UDim2(0.5f,0,0,0);
    	inp.size = new UDim2(0.5f,1,-4,0);
    	inp.textSubmitted += (s) => {
    		if (int.TryParse(s.placeholderText, out int val))
    		{
    			inp.GetMeta<PropertyInfo>()?.SetValue(inp.GetMeta<object>(1), val);
    			HandleCustomInspectorSetProperty(inp.GetMeta<object>(1), inp.GetMeta<PropertyInfo>());
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

    private CustomPanel CreateObjectSelection(PropertyInfo prop, object component, GameObject? value, bool refreshHierarchy = false, int renderLayerOffset = 0)
    {
    	string key = prop.Name;

    	CustomPanel panel = new CustomPanel(prop, component);
    	panel.renderLayer = 5 + renderLayerOffset;
    	panel.position = new UDim2(0,0,3,0);
    	panel.size = new UDim2(1f,0,-6,40);
		panel.borderWidth = 2;
		panel.themeOverride.SetColour("panelOutline", BearingColour.Transparent);
    	gameObject.AddComponent(panel);

    	CustomPanel namePanel = new CustomPanel();
    	namePanel.theme = UIManager.themes["ListItems"];
    	namePanel.parent = panel.rid;
    	namePanel.renderLayer = 6 + renderLayerOffset;
    	namePanel.position = new UDim2(0,0,4,0);
    	namePanel.size = new UDim2(0.5f,1,-8,0);
		namePanel.borderWidth = 2;
		namePanel.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(namePanel);

    	UILabel name = new UILabel();
    	name.theme = UIManager.themes["ListItems"];
    	name.renderLayer = 7 + renderLayerOffset;
    	name.parent = namePanel.rid;
    	name.position = new UDim2(0,0,2,2);
    	name.size = new UDim2(1,1,-4,-4);
    	name.text = key;
    	name.mouseCaptureMode = UIMouseCaptureMode.HandleAndPass;
    	gameObject.AddComponent(name);

    	CustomButton valButton = new CustomButton();
    	valButton.theme = UIManager.themes["ListItems"];
    	valButton.parent = panel.rid;
    	valButton.renderLayer = 6 + renderLayerOffset;
    	valButton.position = new UDim2(0.5f,0,0,0);
    	valButton.size = new UDim2(0.5f,1,-4,0);
		valButton.borderWidth = 2;
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
    	valText.theme = UIManager.themes["ListItems"];
    	valText.renderLayer = 7 + renderLayerOffset;
    	valText.parent = valButton.rid;
    	valText.position = new UDim2(0,0,2,2);
    	valText.size = new UDim2(1,1,-4,-4);
    	valText.text = value is null ? "NULL" : value.name;
    	valText.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
    	gameObject.AddComponent(valText);

    	valButton.metadata = new object[] { prop, component, valText };

    	return panel;
    }

    public void UpdateComponentView()
    {
    	scroll.ClearContents();

    	int id = 1;
    	int final = dropdownTree.Count;
    	foreach (var kvp in dropdownTree.ToDictionary())
    	{
    		kvp.Key.Cleanup();

    		if (id == final)
    			kvp.Value.Cleanup();
    		id++;
    	}
    	dropdownTree.Clear();

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