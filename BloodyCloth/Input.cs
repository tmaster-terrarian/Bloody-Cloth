using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BloodyCloth;

public static class Input
{
    private static KeyboardState currentKeyboardState;
    private static KeyboardState previousKeyboardState;

    public static KeyboardState KeyboardState => currentKeyboardState;

    public static KeyboardState RefreshKeyboardState()
    {
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();

        return currentKeyboardState;
    }

    private static MouseState currentMouseState;
    private static MouseState previousMouseState;

    public static MouseState MouseState => currentMouseState;

    public static MouseState RefreshMouseState()
    {
        previousMouseState = currentMouseState;
        currentMouseState = Mouse.GetState();

        return currentMouseState;
    }

    private static readonly GamePadState[] currentGamepadStates = new GamePadState[4];
    private static readonly GamePadState[] previousGamepadStates = new GamePadState[4];

    public static GamePadState RefreshGamePadState() => RefreshGamePadState(PlayerIndex.One);

    public static GamePadState RefreshGamePadState(PlayerIndex index)
    {
        for(var i = 0; i < 4; i++)
        {
            previousGamepadStates[i] = currentGamepadStates[i];
            currentGamepadStates[i] = GamePad.GetState((PlayerIndex)i);
        }

        return currentGamepadStates[(int)index];
    }

    public static JoystickState GetJoystickState() => GetJoystickState(PlayerIndex.One);

    public static JoystickState GetJoystickState(PlayerIndex index)
    {
        return Joystick.GetState((int)index);
    }

    public static bool GetDown(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !Main.IsPaused;
    }

    public static bool GetPressed(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key) && !Main.IsPaused;
    }

    public static bool GetReleased(Keys key)
    {
        return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key) && !Main.IsPaused;
    }

    public static bool GetDown(Buttons button, PlayerIndex index)
    {
        return currentGamepadStates[(int)index].IsButtonDown(button) && !Main.IsPaused;
    }

    public static bool GetPressed(Buttons button, PlayerIndex index)
    {
        return currentGamepadStates[(int)index].IsButtonDown(button) && !previousGamepadStates[(int)index].IsButtonDown(button) && !Main.IsPaused;
    }

    public static bool GetReleased(Buttons button, PlayerIndex index)
    {
        return !currentGamepadStates[(int)index].IsButtonDown(button) && previousGamepadStates[(int)index].IsButtonDown(button) && !Main.IsPaused;
    }

    public static bool GetDown(Buttons button) => GetDown(button, PlayerIndex.One);

    public static bool GetPressed(Buttons button) => GetPressed(button, PlayerIndex.One);

    public static bool GetReleased(Buttons button) => GetReleased(button, PlayerIndex.One);

    public static bool GetDown(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Pressed && !Main.IsPaused;
    }

    public static bool GetPressed(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Pressed && GetMouseButtonState(previousMouseState, button) == ButtonState.Released && !Main.IsPaused;
    }

    public static bool GetReleased(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Released && GetMouseButtonState(previousMouseState, button) == ButtonState.Pressed && !Main.IsPaused;
    }

    private static ButtonState GetMouseButtonState(MouseState state, MouseButtons button)
    {
        return button switch
        {
            MouseButtons.LeftButton => state.LeftButton,
            MouseButtons.RightButton => state.RightButton,
            MouseButtons.MiddleButton => state.MiddleButton,
            MouseButtons.XButton1 => state.XButton1,
            MouseButtons.XButton2 => state.XButton2,
            _ => ButtonState.Released,
        };
    }

    public static ButtonState GetMouseButtonState(MouseButtons button) => GetMouseButtonState(currentMouseState, button);

    public static int GetScrollDelta()
    {
        return Main.IsPaused ? 0 : System.Math.Sign(currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue);
    }
}

public enum MouseButtons
{
    LeftButton,
    RightButton,
    MiddleButton,
    XButton1,
    XButton2
}
