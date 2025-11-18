using OpenTK.Mathematics;

namespace Bearing;

public class SpriteSheetManager
{
    public Dictionary<Resource, SpriteSheet> sheets = new Dictionary<Resource, SpriteSheet>();

    public void LoadSpriteSheet(Resource sheet, int sliceWidth, int sliceHeight)
    {
        sheets.Add(sheet, new SpriteSheet(sheet, sliceWidth, sliceHeight));
    }

    public Texture GetSprite(Resource sheet, Vector2 position)
    {
        int index = (int)(position.X + sheets[sheet].sWidth * position.Y);

        return sheets[sheet].textures[index];
    }
}