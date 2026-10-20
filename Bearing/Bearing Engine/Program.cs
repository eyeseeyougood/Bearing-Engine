using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using OpenTK.Mathematics;

namespace Bearing;

public static class Program
{
    public static IWindow? window = null;

    private static bool wantsToClose = false;

    static void Main(string[] args)
    {
        Console.WriteLine("Starting Bearing Engine");

        SceneSettingsManager.Init();

        string windowTitle = "Bearing Engine";

        if (SceneSettingsManager.SettingsExist("General"))
        {
            Dictionary<string, string>? settings = SceneSettingsManager.GetSettings("General");

            if (settings is not null)
                if (settings.ContainsKey("windowTitle"))
                    windowTitle = settings["windowTitle"];
        }

        WindowOptions options = WindowOptions.Default with {
            Size = new Silk.NET.Maths.Vector2D<int>(800, 600),
            Title = windowTitle,
        };

        window = Window.Create(options);

        Silk.NET.Maths.Vector2D<int> mainMonitorCentre = Silk.NET.Windowing.Monitor.GetMainMonitor(window).Bounds.Center;
        window.Position = mainMonitorCentre - new Silk.NET.Maths.Vector2D<int>(400, 300);

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Resize += OnResize;
        window.Closing += OnClose;

        window.Run();
    }

    public static Vector2i GetClientSize()
    {
        if (window is null)
            return Vector2i.Zero;

        return new Vector2i(window.Size.X, window.Size.Y);
    }

    public static void OnLoad()
    {
        if (window is null)
            throw new Exception("SOMETHING IS HORRIBLY WRONG!!! Program.cs.OnLoad() should not be called before the window has been created!");

        GLContext.gl = window.CreateOpenGL();

        Input.Init(window.CreateInput());

        new Game();
        Game.instance.OnResize(new Vector2(window.Size.X, window.Size.Y));
        Game.instance.SetTitleChangeable(true);

        Input.LinkToGame();
    }

    public static void Close()
    {
        wantsToClose = true;
    }

    public static void SetClipboard(string text)
    {
        Glfw glfw = Glfw.GetApi();

        if (window is null)
        {
            Logger.LogError("Cannot set the clipboard contents before window creation has occured.");
            return;
        }

        unsafe
        {
            glfw.SetClipboardString(
                (WindowHandle*)window.Handle,
                text
            );
        }
    }

    public static string? GetClipboard()
    {
        Glfw glfw = Glfw.GetApi();

        if (window is null)
        {
            Logger.LogError("Cannot get the clipboard contents before window creation has occured.");
            return "";
        }

        string? result = null;

        unsafe
        {
            result = glfw.GetClipboardString(
                (WindowHandle*)window.Handle
            );
        }

        return result;
    }

    public static void OnClose()
    {
        Game.instance.Cleanup();
    }

    public static void Retitle(string newTitle)
    {
        if (window is null)
        {
            Logger.LogError("Cannot set the window title before the window has been created! Try using the onTitleChangeableChanged event to detect when the title can be changed safely.");
            return;
        }

        window.Title = newTitle;
    }

    public static void OnUpdate(double delta)
    {
        if (window is null)
            return;

        if (!window.IsClosing)
            Game.instance.OnTick(delta);

        // this request system ensure the game doesn't close mid-tick
        // this is important since closing mid-tick can cause memory errors

        if (!window.IsClosing && wantsToClose)
        {
            window.Close();
        }
    }

    public static void OnRender(double delta)
    {
        Game.instance.OnRender(delta);
    }

    public static void OnResize(Silk.NET.Maths.Vector2D<int> newSize)
    {
        Game.instance.OnResize(new Vector2(newSize.X, newSize.Y));
    }
}