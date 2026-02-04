namespace Bearing;

public class SpriteSheet
{
    public List<Texture> textures = new List<Texture>();

    public int sWidth=1;
    public int sHeight=1;

    public SpriteSheet() { }
    public SpriteSheet(Resource spriteSheet, int sliceWidth, int sliceHeight, int take = -1) { Slice(spriteSheet, sliceWidth, sliceHeight, take); }
    public SpriteSheet(Texture spriteSheet, int sliceWidth, int sliceHeight, int take = -1) { Slice(spriteSheet, sliceWidth, sliceHeight, take); }

    ///<Summary>
    ///Take describes how many slices to keep (used to remove blank slices)
    ///</Summary>
    public void Slice(Resource spriteSheet, int sliceWidth, int sliceHeight, int take = -1)
    {
        Texture t = Texture.LoadFromFile(spriteSheet.fullpath, Silk.NET.OpenGL.TextureMinFilter.Nearest, Silk.NET.OpenGL.TextureMagFilter.Nearest, Silk.NET.OpenGL.TextureWrapMode.ClampToEdge);
        Slice(t, sliceWidth, sliceHeight, take);
        t.Dispose();
    }

    ///<Summary>
    ///Take describes how many slices to keep (used to remove blank slices)
    ///</Summary>
    public void Slice(Texture spriteSheet, int sliceWidth, int sliceHeight, int take = -1)
    {
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
}
