using System.Runtime.InteropServices;

namespace Bearing;

public class Sprite : IMetadata
{
    public object[] metadata { get; set; } = new object[0];

    private List<Texture> textures = new List<Texture>();
    private int currentTexture = 0;

    private void ClearTextures()
    {
        foreach (Texture texture in textures)
        {
            texture.Dispose();
        }
        textures.Clear();
    }

    public void SetTexture(Texture texture)
    {
        ClearTextures();

        textures.Add(texture);
    }

    public void SetAnimation(SpriteSheet sheet)
    {
        SetAnimation(sheet.textures);
    }

    public void SetAnimation(List<Texture> frames)
    {
        ClearTextures();

        textures = frames;
    }

    public void SetAnimation(Resource containingFolder)
    {
        SetAnimation(containingFolder.fullpath);
    }

    public void SetAnimation(string containingFolder)
    {
        List<Texture> frames = new List<Texture>();

        foreach (string path in Resources.GetFiles(containingFolder))
        {
            Texture t = Texture.LoadFromFile(path);
            frames.Add(t);
        }

        SetAnimation(frames);
    }

    public Texture Peak()
    {
        if (currentTexture < 0 || currentTexture >= textures.Count)
        {
            Logger.LogError("Index outside bounds of the array: Attempt to get sprite texture which doesn't exist.");
            return null;
        }

        return textures[currentTexture];
    }

    public void ResetPosition(int startFrame = 0)
    {
        currentTexture = startFrame;
    }

    public void Skip(int num = 1)
    {
        currentTexture+=num;
    }

    public void Back(int num = 1)
    {
        currentTexture-=num;
    }

    public Texture Cycle()
    {
        Texture t = Peak();
        currentTexture++;
        if (currentTexture >= textures.Count)
            currentTexture = 0;
        return t;
    }

    public int GetAnimIndex()
    {
        return currentTexture;
    }

    public void Cleanup()
    {
        currentTexture = 0;
        ClearTextures();
    }
}