using Silk.NET.Input;
using Silk.NET.OpenGL;
using Bearing.Multiplayer;
using OpenTK.Mathematics;

namespace Bearing;

public class Game
{
    public static Game instance = null;

    public Scene root;
    private IRenderableComparer renderSorter = new IRenderableComparer();
    private List<IRenderable> renderables = new List<IRenderable>();
    // this code is currently undergoing a refactor, and so doesnt make loads of sense
    // to sum this up, it is going to be split into opaque and transparent objects
    // with the transparent list being sorted, and the opaque one being unsorted
    // for the time being, the renderables list is un-used and so transparent objects do not render

    public int renderPasses = 2;

    public event Action gameTick = () => {};
    public event Action rootLoaded = () => {};

    public Vector2 ClientSize;

    public Camera camera;

    public void AddRenderable(IRenderable renderable)
    {
        renderables.Add(renderable);
        renderables.Sort(renderSorter);
    }

    public void RemoveRenderable(IRenderable renderable)
    {
        renderables.Remove(renderable);
    }

    private int currentRenderableID = -1;
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

    public Game()
    {
        instance = this;

        camera = new Camera(new Vector3(0,2,4f), 8f/6f);

        // init stuff
        Time.Init();

        SceneSettingsManager.Init();

        Gizmos.Init();

        AudioManager.Init();

        MultiplayerManager.Init();

        PhysicsManager.tps = 60;
        PhysicsManager.Init();

        Physics2D.Physics2DManager.Init();

        root = new Scene(SceneLoader.LoadFromFile(@"./Resources/Scene/main.json2"));

        rootLoaded.Invoke();

        UIManager.Init();
    }

    public void OnTextInput(IKeyboard keyboard, char c)
    {
        Input.UpdateKeyPress(char.ConvertToUtf32(c.ToString(),0));
    }

    public void SetClearColour(BearingColour colour)
    {
        float r,g,b,a;
        colour.GetZeroTo255A().Deconstruct(out r, out g, out b, out a);
        GLContext.gl.ClearColor(System.Drawing.Color.FromArgb((int)a,(int)r,(int)g,(int)b));
    }

    public void Cleanup()
    {
        root.Cleanup();
        AudioManager.Cleanup();
    }

    public void OnTick(double dt)
    {
        MultiplayerManager.Tick((float)dt);

        SceneLoader.Tick();
        gameTick.Invoke();
        root.Tick((float)dt);

        Input.Tick((float)dt);
    }

    public void OnRender(double dt)
    {
        GL GL = GLContext.gl;

        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.DepthFunc(DepthFunction.Lequal);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        for (int rp = 0; rp < renderPasses; rp++)
        {
            // TODO: OPTIMISATION
            // this could somehow be optimised to only go over the renderables which haven't already been gone over
            // unless I decide that I want renderables to be able to be drawn in multiple passes ¯\_(ツ)_/¯
            foreach (IRenderable renderable in renderables)
            {
                if (renderable.renderPass == rp)
                    renderable.Render();
            }
        }

        Gizmos.Render();
        
        GL.Disable(EnableCap.DepthTest);
        UIManager.RenderUI();
    }

    public void OnResize(Vector2 newSize)
    {
        if (camera.fitAspectToScreen)
            camera.AspectRatio = newSize.X / newSize.Y;

        ClientSize = newSize;

        GLContext.gl.Viewport(new System.Drawing.Size((int)newSize.X, (int)newSize.Y));
    }
}