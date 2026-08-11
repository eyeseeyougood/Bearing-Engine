using System.Reflection;
using Bearing;

public class Plugin : Component
{
	public string
		internalName,
		displayName,
		description,
		author,
		version,
		lastUpdated,
		firstReleased;

	public string[] links;

	public Plugin(
		string internalName,
		string displayName,
		string description,
		string author,
		string version,
		string lastUpdated,
		string firstReleased,
		params string[] links
		) {
		this.internalName = internalName;
		this.displayName = displayName;
		this.description = description;
		this.author = author;
		this.version = version;
		this.lastUpdated = lastUpdated;
		this.firstReleased = firstReleased;
		this.links = links;
	}

	public bool isEnabled { get; protected set; }
	public bool onByDefault { get; protected set; }

    public sealed override void OnLoad() {}
    public sealed override void OnTick(float dt) {}
    public sealed override void Cleanup() {}

    public void Tick(float dt) { if (isEnabled) {OnTicked(dt);} }
    public void Load() { OnLoaded(); }
    public void Enable() { OnEnabled(); }
    public void Disable() { OnDisabled(); }

    protected virtual void OnLoaded() {}
    protected virtual void OnEnabled() { isEnabled = true; }
    protected virtual void OnDisabled() { isEnabled = false; }
    protected virtual void OnTicked(float dt) {}

    public object? CallAPIMethod(string methodName, params object?[]? args)
    {
    	MethodInfo? m = GetType().GetMethod(methodName);

    	if (m is null)
    	{
    		Logger.LogError($"Attempt to call missing plugin API method '{methodName}()' on plugin '{internalName}' ({displayName})");
    		return null;
    	}

    	if (m.GetCustomAttribute<APIMethodAttribute>() is null)
    	{
    		Logger.LogError($"Attempt to call missing plugin API method '{methodName}()' on plugin '{internalName}' ({displayName}). If you are the developer of this plugin, have you forgotten to attribute the method with [APIMethod]?");
    		return null;
    	}

    	return m.Invoke(this, args);
    }
}