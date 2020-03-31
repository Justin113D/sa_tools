using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Key = System.Windows.Input.Key;
using TKey = OpenTK.Input.Key;

namespace SonicRetro.SAModel.Graphics.OpenGL
{
	public class GLInputUpdater : InputUpdater
	{
		private static readonly Dictionary<Key, TKey> Keymap = new Dictionary<Key, TKey>()
		{
			{ Key.None , TKey.Unknown },
			{ Key.LeftShift , TKey.LShift },
			{ Key.RightShift , TKey.RShift },
			{ Key.LeftCtrl , TKey.LControl },
			{ Key.RightCtrl , TKey.RControl },
			{ Key.LeftAlt , TKey.LAlt },
			{ Key.RightAlt , TKey.RAlt },
			{ Key.LWin , TKey.LWin },
			{ Key.RWin , TKey.RWin },
			{ Key.F1 , TKey.F1 },
			{ Key.F2 , TKey.F2 },
			{ Key.F3 , TKey.F3 },
			{ Key.F4 , TKey.F4 },
			{ Key.F5 , TKey.F5 },
			{ Key.F6 , TKey.F6 },
			{ Key.F7 , TKey.F7 },
			{ Key.F8 , TKey.F8 },
			{ Key.F9 , TKey.F9 },
			{ Key.F10 , TKey.F10 },
			{ Key.F11 , TKey.F11 },
			{ Key.F12 , TKey.F12 },
			{ Key.F13 , TKey.F13 },
			{ Key.F14 , TKey.F14 },
			{ Key.F15 , TKey.F15 },
			{ Key.F16 , TKey.F16 },
			{ Key.F17 , TKey.F17 },
			{ Key.F18 , TKey.F18 },
			{ Key.F19 , TKey.F19 },
			{ Key.F20 , TKey.F20 },
			{ Key.F21 , TKey.F21 },
			{ Key.F22 , TKey.F22 },
			{ Key.F23 , TKey.F23 },
			{ Key.F24 , TKey.F24 },
			{ Key.Up , TKey.Up },
			{ Key.Left , TKey.Left },
			{ Key.Right , TKey.Right },
			{ Key.Down , TKey.Down },
			{ Key.Enter, TKey.Enter },
			{ Key.Escape , TKey.Escape },
			{ Key.Space , TKey.Space },
			{ Key.Tab , TKey.Tab },
			{ Key.Back , TKey.Back },
			{ Key.Insert , TKey.Insert },
			{ Key.Delete , TKey.Delete },
			{ Key.PageUp , TKey.PageUp },
			{ Key.PageDown , TKey.PageDown },
			{ Key.Home , TKey.Home },
			{ Key.End , TKey.End },
			{ Key.CapsLock , TKey.CapsLock },
			{ Key.Scroll , TKey.ScrollLock },
			{ Key.PrintScreen , TKey.PrintScreen },
			{ Key.Pause , TKey.Pause },
			{ Key.NumLock , TKey.NumLock },
			{ Key.Clear , TKey.Clear },
			{ Key.Sleep , TKey.Sleep },
			{ Key.NumPad0 , TKey.Keypad0 },
			{ Key.NumPad1 , TKey.Keypad1 },
			{ Key.NumPad2 , TKey.Keypad2 },
			{ Key.NumPad3 , TKey.Keypad3 },
			{ Key.NumPad4 , TKey.Keypad4 },
			{ Key.NumPad5 , TKey.Keypad5 },
			{ Key.NumPad6 , TKey.Keypad6 },
			{ Key.NumPad7 , TKey.Keypad7 },
			{ Key.NumPad8 , TKey.Keypad8 },
			{ Key.NumPad9 , TKey.Keypad9 },
			{ Key.Divide , TKey.KeypadDivide },
			{ Key.Multiply , TKey.KeypadMultiply },
			{ Key.Subtract , TKey.KeypadSubtract },
			{ Key.Add , TKey.KeypadAdd },
			{ Key.Decimal , TKey.KeypadDecimal },
			{ Key.A , TKey.A },
			{ Key.B , TKey.B },
			{ Key.C , TKey.C },
			{ Key.D , TKey.D },
			{ Key.E , TKey.E },
			{ Key.F , TKey.F },
			{ Key.G , TKey.G },
			{ Key.H , TKey.H },
			{ Key.I , TKey.I },
			{ Key.J , TKey.J },
			{ Key.K , TKey.K },
			{ Key.L , TKey.L },
			{ Key.M , TKey.M },
			{ Key.N , TKey.N },
			{ Key.O , TKey.O },
			{ Key.P , TKey.P },
			{ Key.Q , TKey.Q },
			{ Key.R , TKey.R },
			{ Key.S , TKey.S },
			{ Key.T , TKey.T },
			{ Key.U , TKey.U },
			{ Key.V , TKey.V },
			{ Key.W , TKey.W },
			{ Key.X , TKey.X },
			{ Key.Y , TKey.Y },
			{ Key.Z , TKey.Z },
			{ Key.D0 , TKey.Number0 },
			{ Key.D1 , TKey.Number1 },
			{ Key.D2 , TKey.Number2 },
			{ Key.D3 , TKey.Number3 },
			{ Key.D4 , TKey.Number4 },
			{ Key.D5 , TKey.Number5 },
			{ Key.D6 , TKey.Number6 },
			{ Key.D7 , TKey.Number7 },
			{ Key.D8 , TKey.Number8 },
			{ Key.D9 , TKey.Number9 },
			{ Key.OemTilde , TKey.Tilde },
			{ Key.OemMinus , TKey.Minus },
			{ Key.OemPlus , TKey.Plus },
			{ Key.OemOpenBrackets , TKey.BracketLeft },
			{ Key.OemCloseBrackets , TKey.BracketRight },
			{ Key.OemSemicolon , TKey.Semicolon },
			{ Key.OemQuotes , TKey.Quote },
			{ Key.OemComma , TKey.Comma },
			{ Key.OemPeriod , TKey.Period },
			{ Key.OemQuestion , TKey.Slash },
			{ Key.OemBackslash , TKey.BackSlash }
		};

		private static MouseButton[] _mouseButtons;
		private static int _lastScroll;

		public GLInputUpdater()
		{
			_mouseButtons = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().Distinct().ToArray();
		}

		public override void PlaceCursor(Point loc)
		{
			base.PlaceCursor(loc);
			Mouse.SetPosition(loc.X, loc.Y);
		}

		public override Dictionary<Key, bool> UpdateKeys()
		{
			Dictionary<Key, bool> dict = new Dictionary<Key, bool>();
			KeyboardState state = Keyboard.GetState();
			foreach (Key k in Keymap.Keys)
			{
				dict.Add(k, state.IsKeyDown(Keymap[k]));
			}
			return dict;
		}

		public override Dictionary<MouseButton, bool> UpdateMouse(bool wasFocused)
		{
			Dictionary<MouseButton, bool> dict = new Dictionary<MouseButton, bool>();
			MouseState state = Mouse.GetCursorState();
			foreach (MouseButton k in _mouseButtons)
			{
				dict.Add(k, state.IsButtonDown((OpenTK.Input.MouseButton)k));
			}

			Point mousePos = new Point(state.X, state.Y);
			CursorDif = wasFocused ? new Point(mousePos.X - CursorLoc.X, mousePos.Y - CursorLoc.Y) : default;
			CursorLoc = mousePos;

			ScrollDif = _lastScroll - state.ScrollWheelValue;
			_lastScroll = state.ScrollWheelValue;

			return dict;
		}
	}
}
