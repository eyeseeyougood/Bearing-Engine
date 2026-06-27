using Silk.NET.OpenGL;
using OpenTK.Mathematics;

namespace Bearing;

public class Tilemap : SpriteRenderer
{
	private SpriteSheet? spriteSheet;

	private Dictionary<string, int> associations = new Dictionary<string, int>();
	private Dictionary<Vector2i, int> tilemap = new Dictionary<Vector2i, int>();

    public Tilemap() : base()
    {
        material = new Material()
        {
            shader = new Shader("eng/tilemap.vert", "eng/tilemap.frag"),

            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", value = new List<object> {0.9f, 0.9f, 0.9f, 1.0f} },
            },
        };
    }

    public override void OnLoad() {
        base.OnLoad();
    }
    public override void OnTick(float dt) {
        base.OnTick(dt);
    }
    public override void Cleanup() {
        base.Cleanup();

        if (spriteSheet is not null)
            spriteSheet.Dispose();
    }

    public override unsafe void Render()
    {
        if (spriteSheet is null) { return; }

        GL GL = GLContext.gl;

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        Transform2D transform = ((Transform2D)gameObject.transform);
        //material.SetShaderParameter("position", transform.position);
        //material.SetShaderParameter("rot", transform.rotation);
        //material.SetShaderParameter("scale", transform.scale);
        material.SetShaderParameter("sliceSize", new Vector2(spriteSheet.sWidth, spriteSheet.sHeight));
        material.SetShaderParameter("sheetSize", new Vector2(spriteSheet.GetWidth(), spriteSheet.GetHeight()));

        material.LoadParameters();


        int tileId = 0;
        foreach (var tile in tilemap)
        {
            GL.Uniform1(material.shader.GetUniformLoc($"tiles[{tileId}].x"), tile.Key.X);
            GL.Uniform1(material.shader.GetUniformLoc($"tiles[{tileId}].y"), tile.Key.Y);
            GL.Uniform1(material.shader.GetUniformLoc($"tiles[{tileId}].index"), tile.Value);
            tileId++;
        }
        material.SetShaderParameter("numTiles", tileId);


        LightManager.AddLightingInfo(material);

        Texture? t = sprite.Peak();

        if (t != null)
            t.Use(TextureUnit.Texture0);

        if (renderBackface)
        {
            GL.Disable(GLEnum.CullFace);
        }
        
        BeforeRender();

        material.Use();

        GL.DrawElements(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    private int SheetCoordToIndex(Vector2i coord)
    {
    	if (spriteSheet is null)
    		throw new Exception("The spritesheet of tilemap has not been set!!");

    	int spriteCountX = spriteSheet.GetWidth() / spriteSheet.sWidth;

        int index = coord.X + coord.Y * spriteCountX;

        return index;
    }

    private void ResetSpriteData()
    {
    	associations.Clear();
    }

    public void SetSpriteSheet(SpriteSheet sheet)
    {
    	ResetSpriteData();

    	spriteSheet = sheet;

        sprite.SetTexture(sheet.GetFullTexture());
    }

    public void NameTile(string name, Vector2i spriteSheetCoord)
    {
    	associations.Add(name, SheetCoordToIndex(spriteSheetCoord));
    }

    public void SetTile(Vector2i tilemapCoord, string tileName)
    {
    	SetTile(tilemapCoord, associations[tileName]);
    }

    public void SetTile(Vector2i tilemapCoord, int spriteIndex)
    {
    	if (!tilemap.ContainsKey(tilemapCoord))
    		tilemap.Add(tilemapCoord, 0);

    	tilemap[tilemapCoord] = spriteIndex;
    }

    public string GetTileNameAt(Vector2i tilemapCoord)
    {
    	// Note: I have not tested this and im not sure that the keys list order actually matches the values list order,
    	// this could lead to this function not working as in tended :|
    	return associations.Keys.ToList()[associations.Values.ToList().IndexOf(GetTileAt(tilemapCoord))];
    }

    public int GetTileAt(Vector2i tilemapCoord)
    {
    	return tilemap[tilemapCoord];
    }
}