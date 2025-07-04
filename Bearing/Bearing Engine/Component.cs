namespace Bearing;

public abstract class Component
{
    public GameObject gameObject;

    public int id;

    public abstract void OnLoad();
    public abstract void OnTick(float dt);
    public abstract void Cleanup();
}