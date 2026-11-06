using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Silk.NET.Input;
using HarfBuzzSharp;
using System.Linq;
using System.Text;
using SkiaSharp;
using System;

namespace Bearing;

public static class UIManager
{
    public static List<UIElement> uiElements = new List<UIElement>();
    private static List<UIElement> hoveredObjects = new List<UIElement>();

    public static event EventHandler<string> uiEvent = (i,v)=>{};

    private static ElementComparer elementComp = new ElementComparer();

    public static UITheme currentTheme = new UITheme();
    public static Dictionary<string, UITheme> themes = new Dictionary<string, UITheme>();

    public static bool cursorOverUI = false;

    public static Mesh2D quadMeshCache = new Mesh2D(EmbeddedResource.GetModel("eng/Quad.obj"));

    private static AudioSource sfxSource;

    public static void AddUI(UIElement element)
    {
        uiElements.Add(element);

        uiElements.Sort(elementComp);
    }

    public static void Sort()
    {
        uiElements.Sort(elementComp);
    }

    public static void RemoveUI(UIElement element)
    {
        uiElements.Remove(element);
    }

    public static void LoadTheme(string alias, Resource themeFile)
    {
        UITheme newTheme = new UITheme();
        newTheme.ImportValues(Resources.ReadAllText(themeFile));
        UIManager.themes.Add(alias, newTheme);
    }

    public static UIElement? FindFromRID(int rid)
    {
        foreach (UIElement element in uiElements)
        {
            if (element.rid == rid)
            {
                return element;
            }
        }

        return null;
    }

    public static UIElement? GetHoveredElement()
    {
        if (hoveredObjects.Count == 0)
            return null;

        return hoveredObjects.Last();
    }

    public static void OnMouseEvent()
    {
        if (GetHoveredElement() is not null)
        {
            GetHoveredElement()?.OnMouseEvent();
        }
    }

    public static void RenderUI()
    {
        foreach (UIElement element in uiElements.ToList())
        {
            element.Render();
        }
        
        // hover
        foreach (var hO in hoveredObjects.ToList())
        {
            if (!((UIElement)hO).active)
            {
                hoveredObjects.Remove(hO);
            }
        }
        cursorOverUI = hoveredObjects.Count > 0;
    }

    public static void SendEvent(object sender, string eventType)
    {
        if (eventType == "MouseEnter")
        {
            if (((UIElement)sender).active)
            {
                hoveredObjects.Add((UIElement)sender);
                hoveredObjects.Sort(elementComp);
            }
        }
        if (eventType == "MouseExit")
        {
            hoveredObjects.Remove((UIElement)sender);
        }

        cursorOverUI = hoveredObjects.Count > 0;

        uiEvent.Invoke(sender, eventType);
    }

    public static void PlaySFX(Resource? resource)
    {
        if (resource is null)
        {
            Logger.Log("Warning: potentially unwanted behaviour! Attempted to play null audio resource.");
            return;
        }

        sfxSource.resource = resource;
        sfxSource.Play();
    }

    public static void Init()
    {
        // audio init
        sfxSource = new AudioSource();
        Game.instance.root.AddComponent(sfxSource);

        // mouse event handling
        Input.onMouseEvent += OnMouseEvent;
    }

    public static class UITextHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct GlyphData
        {
            public Vector2 atlasPosition;
            public Vector2 atlasPercSize;
            public Vector2 atlasPixelSize;
            public Vector2 bearing;
            public Vector3 spacing;
            public float advance;
        }

        public static Dictionary<string, Texture> atlasCache = new Dictionary<string, Texture>();
        public static Dictionary<string, GlyphData[]> glyphCache = new Dictionary<string, GlyphData[]>();

        public static void CleanFontCache(Resource font)
        {/*
            if (atlasCache.ContainsKey(font))
                atlasCache.Remove(font);

            if (glyphCache.ContainsKey(font))
                glyphCache.Remove(font);*/
        }

        public static Texture RenderCharacterAtlas(Resource font, int fontSize, uint maxCharacterID = 1000)
        {
            Logger.Log("atlas count: " + atlasCache.Count);
            if (atlasCache.ContainsKey(font.fullpath))
                return atlasCache[font.fullpath];

            GlyphData[] glyphData = new GlyphData[maxCharacterID+1];

            // TODO: TEST IF THESE TWO STREAMS ARE PROPERLY DISPOSED OF
            Blob blob = Blob.FromStream(Resources.Open(font));
            Face face = new Face(blob,0);
            Font hbFont = new Font(face);
            hbFont.SetScale(fontSize,fontSize);

            SKTypeface skFace = SKTypeface.FromStream(Resources.Open(font));
            SKFont skFont = new SKFont(skFace,fontSize);

            int maxWidth = (int)skFont.Metrics.MaxCharacterWidth;
            int maxHeight = (int)(fontSize);

            int atlasColumns = (int)Math.Round(Math.Sqrt((float)(maxCharacterID+1) * maxHeight / maxWidth));

            SKBitmap bmp = new(maxWidth * atlasColumns, ((int)(maxCharacterID + 1) / atlasColumns + 1) * maxHeight);
            using SKCanvas canvas = new(bmp);
            canvas.Clear(SKColors.Transparent);

            using SKPaint paint = new()
            {
                Color = SKColors.White,
                IsAntialias = true,
            };

            for (int id = 0; id <= maxCharacterID; id++)
            {
                hbFont.TryGetGlyph((uint)id, out uint glyph);
                hbFont.TryGetGlyphExtents(glyph, out GlyphExtents extents);
                int bearingY = extents.YBearing;

                int px = (id % atlasColumns) * maxWidth;
                int py = (id / atlasColumns) * maxHeight;

                canvas.DrawText((char)id+"", px, py + bearingY, skFont, paint);

                glyphData[id] = new GlyphData(){
                    atlasPosition = new Vector2(px/(float)bmp.Width,(py-1)/(float)bmp.Height),
                    atlasPercSize = new Vector2(extents.Width/(float)bmp.Width, -(extents.Height-1)/(float)bmp.Height),
                    atlasPixelSize = new Vector2(extents.Width, -extents.Height),
                    bearing = new Vector2(extents.XBearing, extents.YBearing),
                    advance = hbFont.GetHorizontalGlyphAdvance(glyph)
                };
            }

            glyphCache.Add(font.fullpath, glyphData);
            
            Texture tex = Texture.FromData(bmp.Width,bmp.Height,bmp.Bytes);
            atlasCache.Add(font.fullpath, tex);

            bmp.Dispose();

            hbFont.Dispose();
            face.Dispose();
            blob.Dispose();

            skFont.Dispose();
            skFace.Dispose();

            return tex;
        }

        public static GlyphData[] GetGlyphData(Resource font)
        {
            if (glyphCache.ContainsKey(font.fullpath))
                return glyphCache[font.fullpath];

            Logger.LogError($"FAILED TO GET GLYPH DATA FOR FONT '{font.fullpath}'. YOU MUST FIRST CALL RenderCharacterAtlas() TO GENERATE GLYPH INFORMATION");
            return new GlyphData[0];
        }

        public static int GetMaxCharacterHeight(string text, GlyphData[] glyphData)
        {
            if (glyphData is null || glyphData.Length == 0)
            {
                Logger.LogError($"FAILED TO GET MAX HEIGHT FROM GLYPH DATA SINCE IT IS EMPTY! Have you checked that the font you are using exists?");
                return -1;
            }

            int maxHeight = (int)glyphData[(int)text[0]].atlasPixelSize.Y;
            
            foreach (char c in text)
            {
                int h = (int)glyphData[(int)c].atlasPixelSize.Y;
                if (h > maxHeight)
                    maxHeight = h;
            }

            return maxHeight;
        }
    }
}