using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing;

public static class Program
{
    public static IWindow window = null;

    private static bool wantsToClose = false;

    static void Main(string[] args)
    {
        Console.WriteLine("Starting Bearing Engine");

        WindowOptions options = WindowOptions.Default with {
            Size = new Silk.NET.Maths.Vector2D<int>(800, 600),
            Title = "Bearing Engine"
        };

        window = Window.Create(options);

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Resize += OnResize;
        window.Closing += OnClose;

        window.Run();
    }

    public static Vector2i GetClientSize()
    {
        return new Vector2i(window.Size.X, window.Size.Y);
    }

    public static void OnLoad()
    {
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

    public static void OnClose()
    {
        Game.instance.Cleanup();
    }

    public static void Retitle(string newTitle)
    {
        window.Title = newTitle;
    }

    public static void OnUpdate(double delta)
    {
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