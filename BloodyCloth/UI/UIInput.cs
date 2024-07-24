using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Iguina.Drivers;

namespace BloodyCloth.UI;

public class UIInput : IInputProvider
{
    private readonly List<int> _textInput = [];
    private const double InitialDelay = 0.45; // Initial delay before repeating input
    private const double RepeatRate = 0.045;  // Rate of repeated input
    private readonly double[] _charsDelay = new double[255];
    private Keys[] _lastPressedKeys = [];

    public void GetKeyboardInput(GameTime gameTime)
    {
        var keysPressed = Input.KeyboardState.GetPressedKeys();

        // get keyboard text input
        {
            HashSet<Keys> _lastKeys = new(_lastPressedKeys);
            _textInput.Clear();
            var currSeconds = gameTime.TotalGameTime.TotalSeconds;

            foreach (var key in keysPressed)
            {
                char keyChar = ConvertKeyToChar(key, Input.KeyboardState);
                int keyCharForDelay = keyChar.ToString().ToLower()[0];
                if (keyChar != '\0')
                {
                    if ((currSeconds > _charsDelay[keyCharForDelay]) || (!_lastKeys.Contains(key)))
                    {
                        _textInput.Add(keyChar);
                        _charsDelay[keyCharForDelay] = currSeconds + (_lastKeys.Contains(key) ? RepeatRate : InitialDelay);
                    }
                }
            }
            _lastPressedKeys = keysPressed;
        }
    }

    public Iguina.Defs.Point GetMousePosition()
    {
        return new(Main.MousePosition.X, Main.MousePosition.Y);
    }

    public int GetMouseWheelChange()
    {
        return Input.GetScrollDelta();
    }

    private static char ConvertKeyToChar(Keys key, KeyboardState state)
    {
        bool shift = state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);
        bool capsLock = state.CapsLock;

        return key switch
        {
            Keys.A => shift ^ capsLock ? 'A' : 'a',
            Keys.B => shift ^ capsLock ? 'B' : 'b',
            Keys.C => shift ^ capsLock ? 'C' : 'c',
            Keys.D => shift ^ capsLock ? 'D' : 'd',
            Keys.E => shift ^ capsLock ? 'E' : 'e',
            Keys.F => shift ^ capsLock ? 'F' : 'f',
            Keys.G => shift ^ capsLock ? 'G' : 'g',
            Keys.H => shift ^ capsLock ? 'H' : 'h',
            Keys.I => shift ^ capsLock ? 'I' : 'i',
            Keys.J => shift ^ capsLock ? 'J' : 'j',
            Keys.K => shift ^ capsLock ? 'K' : 'k',
            Keys.L => shift ^ capsLock ? 'L' : 'l',
            Keys.M => shift ^ capsLock ? 'M' : 'm',
            Keys.N => shift ^ capsLock ? 'N' : 'n',
            Keys.O => shift ^ capsLock ? 'O' : 'o',
            Keys.P => shift ^ capsLock ? 'P' : 'p',
            Keys.Q => shift ^ capsLock ? 'Q' : 'q',
            Keys.R => shift ^ capsLock ? 'R' : 'r',
            Keys.S => shift ^ capsLock ? 'S' : 's',
            Keys.T => shift ^ capsLock ? 'T' : 't',
            Keys.U => shift ^ capsLock ? 'U' : 'u',
            Keys.V => shift ^ capsLock ? 'V' : 'v',
            Keys.W => shift ^ capsLock ? 'W' : 'w',
            Keys.X => shift ^ capsLock ? 'X' : 'x',
            Keys.Y => shift ^ capsLock ? 'Y' : 'y',
            Keys.Z => shift ^ capsLock ? 'Z' : 'z',
            Keys.D0 => shift ? ')' : '0',
            Keys.D1 => shift ? '!' : '1',
            Keys.D2 => shift ? '@' : '2',
            Keys.D3 => shift ? '#' : '3',
            Keys.D4 => shift ? '$' : '4',
            Keys.D5 => shift ? '%' : '5',
            Keys.D6 => shift ? '^' : '6',
            Keys.D7 => shift ? '&' : '7',
            Keys.D8 => shift ? '*' : '8',
            Keys.D9 => shift ? '(' : '9',
            Keys.Space => ' ',
            Keys.OemPeriod => shift ? '>' : '.',
            Keys.OemComma => shift ? '<' : ',',
            Keys.OemQuestion => shift ? '?' : '/',
            Keys.OemSemicolon => shift ? ':' : ';',
            Keys.OemQuotes => shift ? '"' : '\'',
            Keys.OemBackslash => shift ? '|' : '\\',
            Keys.OemOpenBrackets => shift ? '{' : '[',
            Keys.OemCloseBrackets => shift ? '}' : ']',
            Keys.OemMinus => shift ? '_' : '-',
            Keys.OemPlus => shift ? '+' : '=',
            _ => '\0',
        };
    }

    public int[] GetTextInput()
    {
        return [.. _textInput];
    }

    public TextInputCommands[] GetTextInputCommands()
    {
        List<TextInputCommands> ret = new();
        var keyboard = Keyboard.GetState();
        var ctrlDown = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
        long millisecondsSinceEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        {
            foreach (var value in Enum.GetValues(typeof(TextInputCommands)))
            {
                var key = _inputTextCommandToKeyboardKey[(int)value];
                long msPassed = millisecondsSinceEpoch - _timeToAllowNextInputCommand[(int)value];
                if (keyboard.IsKeyDown(key))
                {
                    if (msPassed > 0)
                    {
                        _timeToAllowNextInputCommand[(int)value] = (millisecondsSinceEpoch + (msPassed >= 250 ? 450 : 45));
                        var command = (TextInputCommands)value;
                        if ((command == TextInputCommands.MoveCaretEnd) && !ctrlDown) { continue; }
                        if ((command == TextInputCommands.MoveCaretEndOfLine) && ctrlDown) { continue; }
                        if ((command == TextInputCommands.MoveCaretStart) && !ctrlDown) { continue; }
                        if ((command == TextInputCommands.MoveCaretStartOfLine) && ctrlDown) { continue; }
                        ret.Add(command);
                    }
                }
                else
                {
                    _timeToAllowNextInputCommand[(int)value] = 0;
                }
            }
        }
        return [.. ret];
    }

    public bool IsMouseButtonDown(MouseButton btn)
    {
        return btn switch
        {
            MouseButton.Left => Input.GetDown(MouseButtons.LeftButton),
            MouseButton.Right => Input.GetDown(MouseButtons.RightButton),
            MouseButton.Wheel => Input.GetDown(MouseButtons.MiddleButton),
            _ => throw new ArgumentOutOfRangeException(nameof(btn))
        };
    }

    static readonly long[] _timeToAllowNextInputCommand = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    static readonly Keys[] _inputTextCommandToKeyboardKey = [
        Keys.Left,
        Keys.Right,
        Keys.Up,
        Keys.Down,
        Keys.Back,
        Keys.Delete,
        Keys.Enter,
        Keys.End,
        Keys.Home,
        Keys.End,
        Keys.Home
    ];
}
