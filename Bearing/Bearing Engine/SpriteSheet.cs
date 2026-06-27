using OpenTK.Mathematics;

namespace Bearing;

public class SpriteSheet
{
    public List<Texture> textures = new List<Texture>();

    private int sheetWidth;
    private int sheetHeight;

    private Texture creationTexture;

    public int sWidth=1; // slice width
    public int sHeight=1; // slice height

    public SpriteSheet() { }
    public SpriteSheet(Resource spriteSheet, int sliceWidth, int sliceHeight, int take = -1) { Slice(spriteSheet, sliceWidth, sliceHeight, take); }
    public SpriteSheet(Texture spriteSheet, int sliceWidth, int sliceHeight, int take = -1) { Slice(spriteSheet, sliceWidth, sliceHeight, take); }

    public Texture GetFullTexture() { return creationTexture; }
    public int GetWidth() { return sheetWidth; }
    public int GetHeight() { return sheetHeight; }

    ///<summary>
    ///This function requires you to specify the slice width and height in the constructor of this spritesheet
    ///</summary>
    public Texture Sample(Vector2i spriteCoord)
    {
        int spriteCountX = sheetWidth / sWidth;

        int index = spriteCoord.X + spriteCoord.Y * spriteCountX;

        return textures[index];
    }

    ///<Summary>
    ///Take describes how many slices to keep (used to remove blank slices)
    ///</Summary>
    public void Slice(Resource spriteSheet, int sliceWidth, int sliceHeight, int take = -1)
    {
        Texture t = Texture.LoadFromResource(Resource.FromPath(spriteSheet.fullpath), Silk.NET.OpenGL.TextureMinFilter.Nearest, Silk.NET.OpenGL.TextureMagFilter.Nearest, Silk.NET.OpenGL.TextureWrapMode.ClampToEdge);
        Slice(t, sliceWidth, sliceHeight, take);
    }

    ///<Summary>
    ///Take describes how many slices to keep (used to remove blank slices)
    ///</Summary>
    public void Slice(Texture spriteSheet, int sliceWidth, int sliceHeight, int take = -1)
    {
        creationTexture = spriteSheet;

        byte[] data = spriteSheet.GetData();

        int width = spriteSheet._width;
        int height = spriteSheet._height;

        int bytesPerPixel = 4;
        int stride = width * bytesPerPixel;
        int sliceStride = sliceWidth * bytesPerPixel;

        List<byte[]> splitData = new List<byte[]>();

        for (int y = 0; y < height; y += sliceHeight)
        {
            for (int x = 0; x < width; x += sliceWidth)
            {
                byte[] slice = new byte[sliceWidth * sliceHeight * bytesPerPixel];

                for (int sy = 0; sy < sliceHeight; sy++)
                {
                    int srcY = y + sy;
                    if (srcY >= height)
                        break;

                    int srcOffset = srcY * stride + x * bytesPerPixel;
                    int dstOffset = sy * sliceStride;

                    int copyLength = Math.Min(sliceStride, stride - x * bytesPerPixel);

                    Buffer.BlockCopy(data, srcOffset, slice, dstOffset, copyLength);
                }

                splitData.Add(slice);
            }
        }

        List<Texture> slices = new List<Texture>();
        foreach (byte[] d in splitData)
        {
            if (take != -1 && slices.Count == take)
                break;
            Texture t = Texture.FromData(sliceWidth, sliceHeight, d, Silk.NET.OpenGL.TextureWrapMode.ClampToEdge, Silk.NET.OpenGL.TextureMinFilter.Nearest, Silk.NET.OpenGL.TextureMagFilter.Nearest);
            slices.Add(t);
        }

        ClearTextures();

        textures = slices;
        sWidth = sliceWidth;
        sHeight = sliceHeight;

        sheetWidth = spriteSheet._width;
        sheetHeight = spriteSheet._height;
    }

    public List<Texture> GetSlices(int start = 0, int count = -1)
    {
        if (count == -1)
            return textures;

        return textures.GetRange(start, count);
    }

    private void ClearTextures()
    {
        foreach (Texture texture in textures)
        {
            texture.Dispose();
        }
        textures.Clear();
    }

    public void Dispose()
    {
        creationTexture.Dispose();
    }
}
