namespace Bearing;

public class ElementComparer : IComparer<UIElement>
{
    public int Compare(UIElement? x, UIElement? y)
    {
        if (x == null || y == null) return 0;

        if (x.renderLayer > y.renderLayer) return 1;

        if (x.renderLayer < y.renderLayer) return -1;

        return 0;
    }
}