using System.Reflection;

namespace Bearing;

public partial class Shader
{
    [CustomInspectorMethod]
    public void OnPropertyChanged(PropertyInfo property)
    {
        if (property.Name == "vert" || property.Name == "frag")
        {
            Cleanup();
            _uniformLocations.Clear();
            InitShader();

            Use();

            foreach (Component c in Hierarchy.instance?.selectedObject?.components)
            {
                if (c is Renderable r)
                {
                    if (r.material.shader == this)
                    {
                        // this slight jank is to redo the attribute pointers now that locations may have changed
                        Game.instance.RemoveRenderable(r);
                        r.OnLoad();
                    }
                }
            }
        }
    }
}