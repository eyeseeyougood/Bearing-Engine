using System.Collections.Generic;
using OpenTK.Mathematics;
using Bearing;

public static class FileAssignableRegistry
{
	public static List<UIButton> registry = new List<UIButton>();

	public static void RegisterFileAssignableButton(UIButton button)
	{
		registry.Add(button);
	}

	public static bool IsButtonFileAssignable(UIButton button)
	{
		return registry.Contains(button);
	}

	public static UIButton? GetButtonAtPosition(Vector2 position)
	{
		UIButton? highest = null;

		foreach (UIButton button in registry)
		{
			if (Extensions.PointInQuad(position, button.GetScreenBoundingBox()))
			{
				if (highest is null)
				{
					highest = button;
					continue;
				}
				if (button.renderLayer > highest.renderLayer)
				{
					highest = button;
				}
			}
		}

		return highest;
	}

	public static void UnregisterFileAssignableButton(UIButton button)
	{
		registry.Remove(button);
	}
}