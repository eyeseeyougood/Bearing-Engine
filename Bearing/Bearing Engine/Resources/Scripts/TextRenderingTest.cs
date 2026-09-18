using OpenTK.Mathematics;
using Silk.NET.OpenGL;
using Bearing;
using Texture = Bearing.Texture;
using Shader = Bearing.Shader;
using HarfBuzzSharp;
using SkiaSharp;

struct GlyphMetrics
{
    public Vector2 origin;
    public Vector2 size;
    public Vector2 bearing;
    public float advance;
    public float padding;
}

public class TextRenderingTest : UIElement
{
	private uint ssbo0;
    private uint ssbo1;

    private string text = "Advancement get! Getting an upgrade!";
    private int atlasColumns = 16;
    private int atlasRows = 0;
    private int atlasWidth = 0;
    private int atlasHeight = 0;
    private int characterOffset = 0;

	public TextRenderingTest(params object[] meta) : base(meta)
    {
        material = new Material()
        {
            shader = new Shader("res/TextTestUI.vert", "res/TextTestUI.frag"),
            parameters = new List<ShaderParam>()
            {
                new ShaderParam() { name = "mainColour", value = new List<object> {1.0f, 1.0f, 1.0f, 1.0f} },
            },
        };
    }

    private void CreateQuadMesh()
    {
    	MeshVertex2D[] verts = new MeshVertex2D[]{
    		new(){ position = new(-0.5f,-0.5f) },
    		new(){ position = new(-0.5f,0.5f) },
    		new(){ position = new(0.5f,0.5f) },
    		new(){ position = new(0.5f,-0.5f) },
    	};

    	uint[] indices = new uint[]{
    		3, 2, 1,
    		1, 0, 3
    	};

    	mesh = Mesh2D.FromData(verts, indices);
    }

    public override void OnLoad()
    {
    	CreateQuadMesh();

    	CreateGlyphBuffer();
        CreateStringBuffer();

        base.OnLoad();

        position = new UDim2(0.2f,0.2f);
        size = new UDim2(0,0,110,120);

        sprite.SetTexture(Texture.LoadFromResource(EmbeddedResource.GetTexture("Test.png")));
    }

    private GlyphMetrics[] GenAtlasAndMetrics(string fontPath, string outputPath, int fontSize = 48)
    {
        Logger.MeasureStart("gen atlas");

        using Blob blob = Blob.FromFile(fontPath);
        using Face face = new Face(blob, 0);
        using Font hbFont = new Font(face);

        hbFont.SetScale(fontSize, fontSize);

        using SKTypeface typeface = SKTypeface.FromFile(fontPath);
        using SKFont skFont = new SKFont(typeface, fontSize);

        using SKPaint paint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.White
        };

        int firstChar = 32;
        int lastChar = 1024;

        int glyphCount = lastChar - firstChar + 1;

        characterOffset = -firstChar;

        int columns = atlasColumns;
        int padding = 30;


        var glyphIDs = new uint[glyphCount];
        var extents = new GlyphExtents[glyphCount];
        var valid = new bool[glyphCount];

        int maxWidth = 0;
        int maxHeight = 0;

        for (int i = 0; i < glyphCount; i++)
        {
            uint codepoint = (uint)(i + firstChar);

            if (!hbFont.TryGetNominalGlyph(codepoint, out uint glyphID))
                continue;

            if (!hbFont.TryGetGlyphExtents(glyphID, out GlyphExtents glyphExtents))
                continue;

            glyphIDs[i] = glyphID;
            extents[i] = glyphExtents;
            valid[i] = true;

            int width = Math.Abs(glyphExtents.Width);
            int height = Math.Abs(glyphExtents.Height);

            maxWidth = Math.Max(maxWidth, width);
            maxHeight = Math.Max(maxHeight, height);
        }

        int cellWidth = maxWidth + padding;
        int cellHeight = maxHeight + padding;

        int rows = (glyphCount + columns - 1) / columns;

        atlasRows = rows;

        atlasWidth = cellWidth * columns;
        atlasHeight = cellHeight * rows;

        using var bitmap = new SKBitmap(
            atlasWidth,
            atlasHeight,
            SKColorType.Rgba8888,
            SKAlphaType.Premul);

        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.Transparent);

        GlyphMetrics[] metricResults = new GlyphMetrics[glyphCount];

        for (int i = 0; i < glyphCount; i++)
        {
            if (!valid[i])
                continue;

            uint glyphID = glyphIDs[i];
            GlyphExtents hbExtents = extents[i];

            char c = (char)(i + firstChar);

            int column = i % columns;
            int row = i / columns;

            float cellX = column * cellWidth;
            float cellY = row * cellHeight;

            // dunno why but harfbuzz seems to give me negative size
            float glyphWidth = Math.Abs(hbExtents.Width);
            float glyphHeight = Math.Abs(hbExtents.Height);

            float glyphX = (cellWidth - glyphWidth) * 0.5f;
            float glyphY = (cellHeight - glyphHeight) * 0.5f;

            float baselineY = cellY + glyphY + hbExtents.YBearing;
            float baselineX = cellX + glyphX - hbExtents.XBearing;

            canvas.DrawText(c.ToString(), baselineX, baselineY, skFont, paint);

            metricResults[i] = new GlyphMetrics
            {
                origin = new Vector2(
                    glyphX,
                    glyphY),

                size = new Vector2(
                    glyphWidth,
                    glyphHeight),

                bearing = new Vector2(
                    hbExtents.XBearing,
                    hbExtents.YBearing),

                advance = hbFont.GetHorizontalGlyphAdvance(glyphID)
            };
        }

        // TODO: Remove this and make the texture a gl texture directly
        Logger.MeasureStart("create file");
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        using var stream = File.Create(outputPath);
        data.SaveTo(stream);
        Logger.MeasureEnd("create file");

        Logger.MeasureEnd("gen atlas");

        return metricResults;
    }

    private unsafe void CreateGlyphBuffer()
    {
    	GL GL = GLContext.gl;

    	ssbo0 = GL.GenBuffer();
    	BindGlyphBuffer();

        GlyphMetrics[] data = GenAtlasAndMetrics("./Andika-Regular.ttf", "Test.png", 48);

        fixed (void* ptr = data)
    	   GL.BufferData(BufferTargetARB.ShaderStorageBuffer, (nuint)(sizeof(GlyphMetrics) * data.Length), ptr, BufferUsageARB.StaticDraw);
    }

    private unsafe void CreateStringBuffer()
    {
        GL GL = GLContext.gl;

        ssbo1 = GL.GenBuffer();
        BindStringBuffer();

        Vector2 firstOffset = new Vector2(0,5);
        Vector2 secondOffset = new Vector2(1,5);

        int[] data = new int[text.Length];
        int i = 0;
        foreach(byte b in System.Text.Encoding.ASCII.GetBytes(text))
        {
            data[i] = b;
            Logger.Log(b);
            i++;
        }

        fixed (void* ptr = data)
           GL.BufferData(BufferTargetARB.ShaderStorageBuffer, (nuint)(sizeof(int) * data.Length), ptr, BufferUsageARB.StaticDraw);
    }

    private void BindGlyphBuffer()
    {
    	GL GL = GLContext.gl;

    	GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, ssbo0);
    }

    private void BindStringBuffer()
    {
        GL GL = GLContext.gl;

        GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 1, ssbo1);
    }

    public override unsafe void Render()
    {
    	UpdateVisibility();

        if (!visible)
            return;

        GL GL = GLContext.gl;

        material.Use();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        material.SetShaderParameter("screenSize", Game.instance.ClientSize);
        material.SetShaderParameter("anchor", anchor);
        material.SetShaderParameter("posOffset", worldPosition.offset);
        material.SetShaderParameter("posScale", worldPosition.scale);
        material.SetShaderParameter("atlasColumns", atlasColumns);
        material.SetShaderParameter("atlasRows", atlasRows);
        material.SetShaderParameter("atlasWidth", atlasWidth);
        material.SetShaderParameter("atlasHeight", atlasHeight);
        material.SetShaderParameter("characterInclusionOffset", characterOffset);
        //material.SetShaderParameter("sizeOffset", worldSize.offset);
        //material.SetShaderParameter("sizeScale", worldSize.scale);

        material.LoadParameters();

        Texture? t = sprite.Peak();

        if (t != null)
            t.Use(TextureUnit.Texture0);

        BeforeRender();

        material.Use();

        BindGlyphBuffer();
        BindStringBuffer();

        //GL.DrawElements(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0);
        GL.DrawElementsInstanced(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, (void*)0, (uint)text.Length);

        AfterRender();
    }
}