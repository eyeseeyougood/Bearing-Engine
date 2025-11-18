using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Input;
using OpenTK.Mathematics;

namespace Bearing;

public static class Input
{
    private static IInputContext input = null;
    public static int currentKeyboard = 0;
    public static int currentMouse = 0;
    public static event Action<string> onCharacterPressed = (i) => { };

    public static void Init(IInputContext context)
    {
        input = context;

        input.Keyboards[0].KeyChar += Game.instance.OnTextInput;
    }

    private static Vector2 prevMousePos = Vector2.Zero;
    private static Dictionary<MouseButton, bool> mbStates = new Dictionary<MouseButton, bool>(); // old copy of key inputs from last frame
    private static Dictionary<Key, bool> kbStates = new Dictionary<Key, bool>();
    public static void Tick(float dt)
    {
        foreach (string button in Enum.GetNames(typeof(MouseButton)))
        {
            Enum.TryParse<MouseButton>(button, out MouseButton v);
            if (!mbStates.ContainsKey(v))
                mbStates.Add(v, false);

            mbStates[v] = input.Mice[currentMouse].IsButtonPressed(v);
        }

        foreach (string key in Enum.GetNames(typeof(Key)))
        {
            Enum.TryParse<Key>(key, out Key v);
            if (!kbStates.ContainsKey(v))
                kbStates.Add(v, false);

            kbStates[v] = input.Keyboards[currentKeyboard].IsKeyPressed(v);
        }

        prevMousePos = new Vector2(input.Mice[currentMouse].Position.X, input.Mice[currentMouse].Position.Y);
    }

    public static void UpdateKeyPress(int unicode)
    {
        onCharacterPressed.Invoke(Encoding.Unicode.GetString(BitConverter.GetBytes(unicode), 0, 4));
    }

    public static bool GetKeyDown(Key key)
    {
        if (!kbStates.ContainsKey(key))
            return false;
        return !kbStates[key] && input.Keyboards[currentKeyboard].IsKeyPressed(key);
    }

    public static bool GetKey(Key key)
    {
        return input.Keyboards[currentKeyboard].IsKeyPressed(key);
    }

    public static bool GetKeyUp(Key key)
    {
        if (!kbStates.ContainsKey(key))
            return false;
        return kbStates[key] && !input.Keyboards[currentKeyboard].IsKeyPressed(key);
    }

    public static bool GetMouseButtonDown(MouseButton button)
    {
        if (!mbStates.ContainsKey(button))
            return false;
        return !mbStates[button] && input.Mice[currentMouse].IsButtonPressed(button);
    }

    public static bool GetMouseButtonDown(int button)
    {
        return GetMouseButtonDown((MouseButton)button);
    }

    public static bool GetMouseButton(MouseButton button)
    {
        return input.Mice[currentMouse].IsButtonPressed(button);
    }

    public static bool GetMouseButton(int button)
    {
        return GetMouseButtonDown((MouseButton)button);
    }

    public static bool GetMouseButtonUp(MouseButton button)
    {
        if (!mbStates.ContainsKey(button))
            return false;
        return mbStates[button] && !input.Mice[currentMouse].IsButtonPressed(button);
    }

    public static bool GetMouseButtonUp(int button)
    {
        return GetMouseButtonUp((MouseButton)button);
    }

    public static Vector2 GetMousePosition()
    {
        if (input.Mice[currentMouse].Cursor.CursorMode == CursorMode.Raw)
        {
            return Game.instance.ClientSize / 2f;
        }

        return new Vector2(input.Mice[currentMouse].Position.X, input.Mice[currentMouse].Position.Y);
    }

    public static Vector2 GetMouseScrollDelta()
    {
        ScrollWheel wheel = input.Mice[currentMouse].ScrollWheels[0];
        return new Vector2(wheel.X, wheel.Y);
    }

    /// <summary>
    /// A function for getting the mouse delta
    /// </summary>
    /// <returns>change in mouse position since the last frame</returns>
    public static Vector2 GetMouseDelta()
    {
        return new Vector2(input.Mice[currentMouse].Position.X, input.Mice[currentMouse].Position.Y) - prevMousePos;
    }

    public static void LockCursor()
    {
        input.Mice[0].Cursor.CursorMode = CursorMode.Raw;
    }

    public static void UnlockCursor()
    {
        input.Mice[0].Cursor.CursorMode = CursorMode.Normal;
    }
}