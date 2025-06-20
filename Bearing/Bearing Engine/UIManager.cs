using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class UIManager
{
    public static List<UIElement> uiElements = new List<UIElement>();

    public static void AddUI(UIElement element)
    {
        uiElements.Add(element);
    }

    public static void RemoveUI(UIElement element)
    {
        uiElements.Remove(element);
    }

    public static void RenderUI()
    {
        foreach (UIElement element in uiElements)
        {
            element.Render();
        }
    }
}
