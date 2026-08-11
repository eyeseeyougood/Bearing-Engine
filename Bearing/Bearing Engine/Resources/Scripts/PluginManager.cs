using System.Reflection;
using Bearing;

public class PluginManager
{
	public static PluginManager? instance;
	public PluginManager() { instance = this; }

	public List<Plugin> loadedPlugins = new List<Plugin>();

	private void EnsureFolder(string path)
	{
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
	}

	///<summary>
	///Finds all of the installed plugins in ./Plugins/ folder
	///</summary>
	public void LoadPlugins()
	{
		string path = "./Plugins/";

		EnsureFolder(path);

		foreach (string f in Directory.GetFiles(path))
		{
			Assembly asm = Assembly.LoadFrom(f);
			Type[] types = asm.GetTypes();

			foreach (Type t in types)
			{
				if (t.IsSubclassOf(typeof(Plugin)))
				{
					object? plugin = null;
					if (t is not null)
						plugin = Activator.CreateInstance(t);

					if (plugin is not null)
					{
						((Plugin)plugin).Load();
						loadedPlugins.Add((Plugin)plugin);
					}
				}
			}
		}

		// separate loop to ensure that all plugins are loaded by the time enable() is called
		foreach (Plugin plugin in loadedPlugins)
			if (plugin.onByDefault)
				plugin.Enable();
	}

	public Plugin? TryGetLoadedPlugin(string internalName)
	{
		foreach (Plugin plugin in loadedPlugins)
		{
			if (plugin.internalName == internalName)
			{
				return plugin;
			}
		}

		return null;
	}

	public object? CallAPI(string pluginInternalName, string methodName, params object?[]? args)
	{
		Plugin? plugin = TryGetLoadedPlugin(pluginInternalName);

		if (plugin is null)
		{
			Logger.LogError("Attempt to call API of '{pluginInternalName}', but the plugin was not found. Are you missing dependencies? Could this be a typo? Perhaps it changed internal name between versions?");
			return null;
		}

		return plugin.CallAPIMethod(methodName, args);
	}

	public void Tick(float dt)
	{
		foreach (Plugin plugin in loadedPlugins)
		{
			plugin.Tick(dt);
		}
	}

	public void TogglePluginEnabled(Plugin plugin)
	{
		if (plugin.isEnabled)
			plugin.Disable();
		else
			plugin.Enable();
	}
}