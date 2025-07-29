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

    public static void RemoveUI(UIElement element)
    {
        uiElements.Remove(element);
    }

    public static UIElement? FindFromID(int rid)
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

    private static bool orderSame = false;
    private static List<int> prevOrder = new List<int>();
    public static void RenderUI()
    {
        List<int> orderCopy = prevOrder.ToList();
        prevOrder.Clear();

        if (!orderSame)
        {
            uiElements.Sort(elementComp);
            Logger.Log("Sorted UI");
        }

        orderSame = true;

        int indx = 0;
        foreach (UIElement element in uiElements)
        {
            element.Render();

            if (indx >= orderCopy.Count)
                continue;

            if (orderCopy[indx] != element.rid)
            {
                orderSame = false;
            }
            prevOrder.Add(element.rid);
            indx++;
        }
    }

    public static void SendEvent(object sender, string eventType)
    {
        uiEvent.Invoke(sender, eventType);
    }

    public static class UITextHelper
    {
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