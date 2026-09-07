using OpenTK.Mathematics;
using Bearing;

public sealed class UILogger : Component
{
    private static UILogger? instance;

    public sealed override void OnLoad() {instance = this; Logger.Log("UILogger initialised");}
    public sealed override void OnTick(float dt) {}
    public sealed override void Cleanup() {}

    ///<summary>
    ///The first button always closes the dialog with no action, while the second button can be given an action to invoke when pressed.
    ///</summary>
    private void CreatePanel(string text, string button1Text, string? button2Text = null, Action<UIButton>? button2Action = null, BearingColour? customTextColour = null)
    {
    	CustomPanel panel = new CustomPanel();
    	panel.theme = UIManager.themes["BigPanels"];
    	panel.renderLayer = 10000;
        panel.anchor = new Vector2(0.5f,0.5f);
        panel.position = new UDim2(0.5f,0.5f);
        panel.size = new UDim2(0.5f,0.5f);
    	gameObject.AddComponent(panel);

        Action<UIButton> closePanel = (b) => {gameObject.RemoveComponent(panel);};

        UILabel message = new UILabel();
        message.theme = UIManager.themes["BigPanels"];
        message.parent = panel.rid;
        message.renderLayer = panel.renderLayer + 1;
        message.position = new UDim2(0,0,10,10);
        message.size = new UDim2(1,0.7f,-20,-20);
        if (customTextColour is not null)
            message.themeOverride.SetColour("labelText", customTextColour);
        message.text = text;
        message.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(message);

        CustomButton button1 = new CustomButton();
        button1.theme = UIManager.themes["BigButtons"];
        button1.parent = panel.rid;
        button1.renderLayer = panel.renderLayer + 1;
        button1.anchor = new Vector2(button2Text is null? 0.5f : 0f,0f);
        button1.position = new UDim2(button2Text is null? 0.5f : 0f,0.7f,button2Text is null? 0 : 10,10);
        button1.size = new UDim2(0.5f,0.3f,button2Text is null? 0 : -15,-20);
        button1.buttonPressed += closePanel;
        gameObject.AddComponent(button1);

        UILabel button1Label = new UILabel();
        button1Label.theme = UIManager.themes["BigButtons"];
        button1Label.parent = button1.rid;
        button1Label.renderLayer = panel.renderLayer + 2;
        button1Label.position = new UDim2(0,0,10,10);
        button1Label.size = new UDim2(1,1,-20,-20);
        button1Label.text = button1Text;
        button1Label.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
        gameObject.AddComponent(button1Label);

        if (button2Text is not null)
        {
            CustomButton button2 = new CustomButton();
            button2.theme = UIManager.themes["BigButtons"];
            button2.parent = panel.rid;
            button2.renderLayer = panel.renderLayer + 1;
            button2.anchor = new Vector2(0f,0f);
            button2.position = new UDim2(0.5f,0.7f,0,10);
            button2.size = new UDim2(0.5f,0.3f,-10,-20);
            button2.buttonPressed += closePanel;
            gameObject.AddComponent(button2);

            UILabel button2Label = new UILabel();
            button2Label.theme = UIManager.themes["BigButtons"];
            button2Label.parent = button2.rid;
            button2Label.renderLayer = panel.renderLayer + 2;
            button2Label.position = new UDim2(0,0,10,10);
            button2Label.size = new UDim2(1,1,-20,-20);
            button2Label.text = button2Text;
            button2Label.mouseCaptureMode = UIMouseCaptureMode.PassThrough;
            gameObject.AddComponent(button2Label);

            if (button2Action is not null)
            {
                button2.buttonPressed += button2Action;
            }
        }

        UIManager.Sort();
    }

    public static void DisplayWarning(string message, string? otherActionText = null, Action<UIButton>? otherAction = null)
    {
    	Logger.Log("UILogger attempted to display warning: '" + message + "'");

        instance?.CreatePanel(message,"Ok",otherActionText, otherAction);
    }

    public static void DisplayError(string message, bool timestamp = true, string? otherActionText = null, Action<UIButton>? otherAction = null)
    {
    	Logger.Log("UILogger attempted to display error: '" + message + "'");

        if (timestamp)
            message = "["+Time.now+"] ERROR: " + message;

    	instance?.CreatePanel(message,"Ok",otherActionText, otherAction, BearingColour.Red);
    }
}