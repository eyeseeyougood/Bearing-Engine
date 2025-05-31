namespace Bearing;

public abstract class Component
{
    public GameObject gameObject;

    public abstract void OnLoad();
    public abstract void OnTick();
    public abstract void Cleanup();
}