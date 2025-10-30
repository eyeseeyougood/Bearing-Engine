using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class UIManager
{
    public static List<UIElement> uiElements = new List<UIElement>();

    public static event EventHandler<string> uiEvent = (i,v)=>{};

    private static ElementComparer elementComp = new ElementComparer();

    public static UITheme currentTheme = new UITheme();

    public static bool cursorOverUI = false;
    /// <summary>
    /// This stores the object that the mouse is currently using
    /// </summary>
    public static object mouseUsingObject;

    public static Mesh2D quadMeshCache = new Mesh2D("Quad.obj", true);

    private static int currentID = -1;
    private static List<int> usedIDs = new List<int>();
    public static int GetUniqueUIID()
    {
        currentID++;
        while (usedIDs.Contains(currentID))
        {
            currentID++;
        }
        return currentID;
    }

    public static void AddUI(UIElement element)
    {
        uiElements.Add(element);
        if (element.rid == -1)
        {
            element.rid = GetUniqueUIID();
        }
        // TODO: MAGIC - FIGURE OUT WHY THIS BREAKS STUFF
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

    public static void RenderUI()
    {
        foreach (UIElement element in uiElements.ToList())
        {
            element.Render();
        }
        
        // hover
        foreach (var hO in hoveredObjects.ToList())
        {
            if (!((UIElement)hO).visible)
            {
                hoveredObjects.Remove(hO);
            }
        }
        cursorOverUI = hoveredObjects.Count > 0;
    }

    private static List<object> hoveredObjects = new List<object>();
    public static void SendEvent(object sender, string eventType)
    {
        if (eventType == "MouseEnter")
        {
            if (((UIElement)sender).visible)
                hoveredObjects.Add(sender);
        }
        if (eventType == "MouseExit")
        {
            hoveredObjects.Remove(sender);
        }

        cursorOverUI = hoveredObjects.Count > 0;

        uiEvent.Invoke(sender, eventType);
    }

    public static class UITextHelper
    {
        public static Dictionary<string, Dictionary<char, float>> charLengths = new Dictionary<string, Dictionary<char, float>>(); // lengths in pixels of each char for a given font (precached)
        public static Dictionary<string, float> fontHeights = new Dictionary<string, float>();

        public static int MeasureText(string text, string font = "Arial")
        {
            if (!charLengths.ContainsKey(font))
                GenCharLookup(font);

            float len = 0;

            foreach (char c in text)
            {
                len += charLengths[font][c];
            }

            return (int)len;
        }

        public static void GenCharLookup(string font)
        {
            var fontSize = 48;
            var typeface = SKTypeface.FromFamilyName(font);
            var paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = fontSize,
            };

            if (!charLengths.ContainsKey(font))
                charLengths.Add(font, new Dictionary<char, float>());

            for (int i = 20; i < 127; i++)
                charLengths[font].Add((char)i, paint.MeasureText(((char)i).ToString()));
        }
        public static Texture RenderTextToBmp(string text, string font = "Arial")
        {
            var fontSize = 48;
            var typeface = SKTypeface.FromFamilyName(font);
            var paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = fontSize,
                IsAntialias = true,
                Color = SKColors.White
            };

            var lines = text.Split('\n');
            var fontMetrics = paint.FontMetrics;
            var lineHeight = fontMetrics.Descent - fontMetrics.Ascent;

            if (!fontHeights.ContainsKey(font))
                fontHeights.Add(font, lineHeight);

            // Determine maximum width and total height
            int width = (int)lines.Max(line => paint.MeasureText(line));
            int height = (int)(lineHeight * lines.Length);

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            for (int i = 0; i < lines.Length; i++)
            {
                float y = -fontMetrics.Ascent + i * lineHeight;
                canvas.DrawText(lines[i], 0, y, paint);
            }

            return Texture.FromData(width, height, bitmap.Bytes, OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToBorder);
        }
    }
}