using OpenTK.Mathematics;
using Bearing;

public partial class SkyboxPlugin : Plugin
{
    private GameObject? skybox;

    public SkyboxPlugin()
    {
        displayName = "Skybox";
        description = "Adds a simple skybox.";
        author = "eyeseeyougood";
        version = "1.0.0";
        releaseDate = "2026-04-12";
        link = "https://github.com/eyeseeyougood";
    }

    public override void OnEnable()
    {
        base.OnEnable();

        skybox = new GameObject();
        skybox.name = "skybox";
        ((Transform3D)skybox.transform).position = Vector3.UnitY * 2f;
        Skybox s = new Skybox(Resource.GetTexture("SunsetCubemap.png"));
        skybox.AddComponent(s);
        skybox.parent = Game.instance.root;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (skybox != null)
        {
            skybox.Cleanup();
            skybox = null;
        }
    }
}