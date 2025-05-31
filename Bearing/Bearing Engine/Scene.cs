namespace Bearing;

public class Scene : GameObject
{
    public Scene(GameObject root)
    {
        name = root.name;
        transform = root.transform;
        immediateChildren = root.immediateChildren;
        parent = root.parent;
    }
}