using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing;

public static class Program
{
    public static IWindow window = null;

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

        window.Run();
    }

    public static void OnLoad()
    {
        GLContext.gl = window.CreateOpenGL();

        new Game();
        Game.instance.OnResize(new Vector2(window.Size.X, window.Size.Y));

        Input.Init(window.CreateInput());
    }

    public static void OnUpdate(double delta)
    {
        Game.instance.OnTick(delta);
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