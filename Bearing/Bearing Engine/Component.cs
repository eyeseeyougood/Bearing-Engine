namespace Bearing;

public abstract class Component : IMetadata
{
    public GameObject gameObject;

    public int id = -1;

    public object[] metadata { get; set; } = new object[0];

    private List<(Action, float)> delays = new List<(Action, float)>();

    public abstract void OnLoad();
    public virtual void OnTick(float dt)
    {
        foreach (var kvp in delays.ToList()) // TODO: OPTIMISATION, i have no idea if .ToList is slow here or not, so just keep it in mind
        {
            if (Time.now > kvp.Item2)
            {
                kvp.Item1.Invoke();
                delays.Remove(kvp);
            }
        } 
    }
    public abstract void Cleanup();

    protected void Delay(Action func, float seconds)
    {
        delays.Add((func, Time.now + seconds));

    }
    protected T Transform<T>()
    {
        return (T)Convert.ChangeType(gameObject.transform, typeof(T));
    }
}