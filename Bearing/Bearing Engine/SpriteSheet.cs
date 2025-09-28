namespace Bearing;

public class SpriteSheet
{
    public List<Texture> textures = new List<Texture>();

    public int sWidth=1;
    public int sHeight=1;

    public SpriteSheet() { }
    public SpriteSheet(Resource spriteSheet, int sliceWidth, int sliceHeight) { Slice(spriteSheet, sliceWidth, sliceHeight); }
    public SpriteSheet(Texture spriteSheet, int sliceWidth, int sliceHeight) { Slice(spriteSheet, sliceWidth, sliceHeight); }

    public void Slice(Resource spriteSheet, int sliceWidth, int sliceHeight)
    {
        Texture t = Texture.LoadFromFile(spriteSheet.fullpath);
        Slice(t, sliceWidth, sliceHeight);
        t.Dispose();
    }

    public void Slice(Texture spriteSheet, int sliceWidth, int sliceHeight)
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
                // allocate one slice
                byte[] slice = new byte[sliceWidth * sliceHeight * bytesPerPixel];

                // copy row by row
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
            Texture t = Texture.FromData(sliceWidth, sliceHeight, d);
            slices.Add(t);
        }

        ClearTextures();

        textures = slices;
        sWidth = sliceWidth;
        sHeight = sliceHeight;
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
