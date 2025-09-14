using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.ComponentModel;
using Bearing.Multiplayer;

namespace Bearing;

public class Game : GameWindow
{
    public static Game instance;

    public Game (int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { }

    public Scene root;
    private List<IRenderable> renderables = new List<IRenderable>();
    // this code is currently undergoing a refactor, and so doesnt make loads of sense
    // to sum this up, it is going to be split into opaque and transparent objects
    // with the transparent list being sorted, and the opaque one being unsorted
    // for the time being, the renderables list is un-used and so transparent objects do not render
    private List<IRenderable> opaqueRenderables = new List<IRenderable>();

    public event Action gameTick = () => {};

    public Camera camera;

    public void AddOpaqueRenderable(IRenderable renderable)
    {
        opaqueRenderables.Add(renderable);
    }

    public void RemoveOpaqueRenderable(IRenderable renderable)
    {
        opaqueRenderables.Remove(renderable);
    }


    private int currentRenderableID=-1;
    public int GetUniqueRenderableID()
    {
        currentRenderableID++;
        return currentRenderableID;
    }

    private int currentGameObjectID = -1;
    public int GetUniqueGameObjectID()
    {
        currentGameObjectID++;
        return currentGameObjectID;
    }

    protected override void OnLoad()
    {
        instance = this;

        camera = new Camera(new Vector3(0,2,4f), 8f/6f);

        // init stuff
        Time.Init();

        SceneSettingsManager.Init();

        Gizmos.Init();

        AudioManager.Init();

        MultiplayerManager.Init();

        // TODO: OPTIMISATION
        PhysicsManager.ticksPerTick = 20; // testing value
        PhysicsManager.Init();
        
        root = new Scene(SceneLoader.LoadFromFile(@"./Resources/Scene/main.json"));
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        Input.UpdateKeyPress(e.Unicode);

        base.OnTextInput(e);
    }

    public void CursorLockStateChanged(bool state)
    {
        if (state)
        {
            CursorState = CursorState.Grabbed;
        }
        else
        {
            CursorState = CursorState.Normal;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        root.Cleanup();
        AudioManager.Cleanup();
        base.OnClosing(e);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        MultiplayerManager.Tick((float)e.Time);

        SceneLoader.Tick();
        AudioManager.Tick();
        Input.UpdateState(KeyboardState, MouseState);
        gameTick.Invoke();
        root.Tick((float)e.Time);
        PhysicsManager.Tick();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.DepthFunc(DepthFunction.Lequal);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach (IRenderable renderable in opaqueRenderables)
        {
            renderable.Render();
        }

        Gizmos.Render();
        
        UIManager.RenderUI();

        SwapBuffers();
    }


    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        
        camera.AspectRatio = e.Width / (float)e.Height;
        
        GL.Viewport(0, 0, e.Width, e.Height);
    }
}