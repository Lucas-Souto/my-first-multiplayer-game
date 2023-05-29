using Microsoft.Xna.Framework.Input;

namespace WS
{
    public static class Input
    {
        private static KeyboardState currentKeyState, oldKeyState;

        public static void Update()
        {
            currentKeyState = oldKeyState;
            oldKeyState = Keyboard.GetState();
        }

        public static bool IsKeyDown(Keys key) => currentKeyState.IsKeyDown(key);
        public static bool IsKeyPressed(Keys key) => currentKeyState.IsKeyDown(key) && oldKeyState.IsKeyUp(key);
        public static bool IsKeyReleased(Keys key) => currentKeyState.IsKeyUp(key) && oldKeyState.IsKeyDown(key);
    }
}