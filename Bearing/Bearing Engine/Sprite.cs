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

    public void SetTexture(string embeddedTextureName, bool cleanupTextures = true)
    {
        SetTexture(Texture.LoadFromResource(EmbeddedResource.GetTexture(embeddedTextureName)), cleanupTextures);
    }

    public void SetTexture(Texture texture, bool cleanupTextures = true)
    {
        if (cleanupTextures)
            ClearTextures();
        else
            textures.Clear();

        textures.Add(texture);
    }

    public void SetAnimation(SpriteSheet sheet, bool cleanupTextures = true)
    {
        SetAnimation(sheet.textures, cleanupTextures);
    }

    public void SetAnimation(List<Texture> frames, bool cleanupTextures = true)
    {
        if (cleanupTextures)
            ClearTextures();
        else
            textures.Clear();


        textures = frames;
    }

    public void SetAnimation(Resource containingFolder, bool cleanupTextures = true)
    {
        SetAnimation(containingFolder.fullpath, cleanupTextures);
    }

    public void SetAnimation(string embeddedContainingFolder, bool cleanupTextures = true)
    {
        List<Texture> frames = new List<Texture>();

        foreach (string path in Resources.GetEmbeddedFiles(embeddedContainingFolder))
        {
            Texture t = Texture.LoadFromResource(EmbeddedResource.FromPath(path));
            frames.Add(t);
        }

        SetAnimation(frames, cleanupTextures);
    }

    public Texture? Peak()
    {
        if (currentTexture < 0 || currentTexture >= textures.Count)
        {
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

    public Texture? Cycle()
    {
        Texture? t = Peak();
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