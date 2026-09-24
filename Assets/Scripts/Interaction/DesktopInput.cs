using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Academy
{
    public static class DesktopInput
    {
        public static bool Escape
        {
            get {
#if ENABLE_INPUT_SYSTEM
                return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
                return Input.GetKeyDown(KeyCode.Escape);
#endif
            }
        }
        public static bool Enter
        {
            get {
#if ENABLE_INPUT_SYSTEM
                return Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
#else
                return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
            }
        }
        public static bool Use
        {
            get {
#if ENABLE_INPUT_SYSTEM
                return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
                return Input.GetKeyDown(KeyCode.E);
#endif
            }
        }
        public static bool Grab => KeyPressed(KeyCode.G);
        public static bool Inspect => KeyPressed(KeyCode.F);
        public static bool Jump => KeyPressed(KeyCode.Space);
        public static bool ToggleCursor => KeyPressed(KeyCode.Tab);
        public static bool Sprint => KeyHeld(KeyCode.LeftShift);
        public static bool Crouch => KeyHeld(KeyCode.LeftControl);
        public static bool RotateHeld => KeyHeld(KeyCode.R);
        private static bool KeyPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current; if (k == null) return false;
            switch (key)
            {
                case KeyCode.G: return k.gKey.wasPressedThisFrame;
                case KeyCode.F: return k.fKey.wasPressedThisFrame;
                case KeyCode.Space: return k.spaceKey.wasPressedThisFrame;
                case KeyCode.Tab: return k.tabKey.wasPressedThisFrame;
                default: return false;
            }
#else
            return Input.GetKeyDown(key);
#endif
        }
        private static bool KeyHeld(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current; if (k == null) return false;
            switch (key)
            {
                case KeyCode.LeftShift: return k.leftShiftKey.isPressed;
                case KeyCode.LeftControl: return k.leftCtrlKey.isPressed;
                case KeyCode.R: return k.rKey.isPressed;
                default: return false;
            }
#else
            return Input.GetKey(key);
#endif
        }
        public static bool Look
        {
            get {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
                return Input.GetMouseButton(1);
#endif
            }
        }
        public static bool Click
        {
            get {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
                return Input.GetMouseButtonDown(0);
#endif
            }
        }
        public static Vector2 Pointer
        {
            get {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
                return Input.mousePosition;
#endif
            }
        }
        public static Vector2 LookDelta
        {
            get {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null ? Mouse.current.delta.ReadValue() * .12f : Vector2.zero;
#else
                return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 2;
#endif
            }
        }
        public static Vector2 Move
        {
            get {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                if (k == null) return Vector2.zero;
                return new Vector2((k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0),
                    (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0));
#else
                return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
            }
        }
    }
}
