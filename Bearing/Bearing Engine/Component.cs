namespace Bearing;

public abstract class Component : IMetadata
{
    public GameObject gameObject;

    public int id = -1;

    public object[] metadata { get; set; } = new object[0];

    public abstract void OnLoad();
    public abstract void OnTick(float dt);
    public abstract void Cleanup();
    protected async Task Delay(Action func, float seconds)
    {
        await Time.WaitTillTime(Time.now + seconds);

        func.Invoke();
    }
}