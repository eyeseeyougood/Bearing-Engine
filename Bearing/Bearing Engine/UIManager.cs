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

    private static ElementComparer elementComp = new ElementComparer();

    public static UITheme currentTheme = new UITheme();

    public static void AddUI(UIElement element)
    {
        uiElements.Add(element);
        uiElements.Sort(elementComp);
    }

    public static void RemoveUI(UIElement element)
    {
        uiElements.Remove(element);
    }

    public static UIElement? FindFromID(int id)
    {
        foreach (UIElement element in uiElements)
        {
            if (element.id == id)
            {
                return element;
            }
        }

        return null;
    }

    public static void RenderUI()
    {
        foreach (UIElement element in uiElements)
        {
            element.Render();
        }
    }

    public static class UITextHelper
    {
        public static Texture RenderTextToBmp(string text)
        {
            var fontSize = 48;
            var typeface = SKTypeface.FromFamilyName("Arial");
            var paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = fontSize,
                IsAntialias = true,
                Color = SKColors.White
            };

            var textWidth = paint.MeasureText(text);
            var fontMetrics = paint.FontMetrics;
            var height = (int)(fontMetrics.Descent - fontMetrics.Ascent);

            using var bitmap = new SKBitmap((int)textWidth, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawText(text, 0, -fontMetrics.Ascent, paint);

            return Texture.FromData((int)textWidth, height, bitmap.Bytes, OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToBorder);
        }
    }
}