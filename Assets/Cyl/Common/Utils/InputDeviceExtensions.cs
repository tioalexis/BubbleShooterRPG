using UnityEngine.InputSystem;

namespace Cyl.Common.Utils
{
    public static class InputDeviceExtensions
    {
        public static bool IsMouseOrTouch(this InputDevice device)
        {
            return device is Mouse or Touchscreen;
        }

        public static bool IsKeyboard(this InputDevice device)
        {
            return device is Keyboard;
        }
    }
}