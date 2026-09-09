using System.Windows.Forms;

namespace TuxSpace.Core;

public static class InputManager
{
    private static HashSet<Keys> keysDown = new();

    public static bool KeyDown(Keys key)
    {
        return keysDown.Contains(key);
    }

    public static void KeyPressed(Keys key)
    {
        keysDown.Add(key);
    }

    public static void KeyReleased(Keys key)
    {
        keysDown.Remove(key);
    }
}