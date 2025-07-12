using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public static class Input
{
    private static KeyboardState kstate;
    private static MouseState mstate;
    public static event Action<string> onCharacterPressed = (i) => { };

    public static void UpdateKeyPress(int unicode)
    {
        onCharacterPressed.Invoke(Encoding.Unicode.GetString(BitConverter.GetBytes(unicode), 0, 4));
    }

    public static void UpdateState(KeyboardState ks, MouseState ms)
    {
        kstate = ks;
        mstate = ms;
    }

    public static bool GetKeyDown(Keys key)
    {
        if (kstate == null) 
            return false;
        return kstate.IsKeyPressed(key);
    }

    public static bool GetKey(Keys key)
    {
        if (kstate == null) 
            return false;
        return kstate.IsKeyDown(key);
    }

    public static bool GetKeyUp(Keys key)
    {
        if (kstate == null) 
            return false;
        return kstate.IsKeyReleased(key);
    }

    public static bool GetMouseButtonDown(MouseButton button)
    {
        if (mstate == null) 
            return false;
        return mstate.IsButtonPressed(button);
    }

    public static bool GetMouseButtonDown(int button)
    {
        if (mstate == null)
            return false;
        return mstate.IsButtonPressed((MouseButton)button);
    }

    public static bool GetMouseButton(MouseButton button)
    {
        if (mstate == null) 
            return false;
        return mstate.IsButtonDown(button);
    }

    public static bool GetMouseButton(int button)
    {
        if (mstate == null)
            return false;
        return mstate.IsButtonDown((MouseButton)button);
    }

    public static bool GetMouseButtonUp(MouseButton button)
    {
        if (mstate == null) 
            return false;
        return mstate.IsButtonReleased(button);
    }

    public static bool GetMouseButtonUp(int button)
    {
        if (mstate == null)
            return false;
        return mstate.IsButtonReleased((MouseButton)button);
    }

    public static Vector2 GetMouseScrollDelta()
    {
        return mstate.ScrollDelta;
    }

    /// <summary>
    /// A function for getting the mouse delta
    /// </summary>
    /// <returns>change in mouse position since the last frame</returns>
    public static Vector2 GetMouseDelta()
    {
        return mstate.Delta;
    }

    public static void LockCursor()
    {
        Game.instance.CursorLockStateChanged(true);
    }

    public static void UnlockCursor()
    {
        Game.instance.CursorLockStateChanged(false);
    }
}