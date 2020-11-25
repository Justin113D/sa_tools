using SonicRetro.SAModel.Graphics.APIAccess;
using System.Collections.Generic;
using System.Windows.Input;
using Point = System.Drawing.Point;

namespace SonicRetro.SAModel.Graphics
{
	/// <summary>
	/// Used to receive input from keyboard and mouse
	/// </summary>
	public class Input
	{
		/// <summary>
		/// Responsible for updating the input
		/// </summary>
		private readonly IGAPIAInput _apiAccess;

		/// <summary>
		/// Last state of each key
		/// </summary>
		private Dictionary<Key, bool> _keyWasPressed;

		/// <summary>
		/// Current state of each key
		/// </summary>
		private Dictionary<Key, bool> _keyPressed;

		/// <summary>
		/// Last state of each mouse button
		/// </summary>
		private Dictionary<MouseButton, bool> _mouseWasPressed;

		/// <summary>
		/// Current state of each mouse button
		/// </summary>
		private Dictionary<MouseButton, bool> _mousePressed;

		/// <summary>
		/// The last read cursor location
		/// </summary>
		public Point CursorPos => _apiAccess.GetCursorPos();

		/// <summary>
		/// The amount that the cursor moved
		/// </summary>
		public Point CursorDif => _apiAccess.GetCursorDif();

		/// <summary>
		/// The amount that the scroll was used 
		/// </summary>
		public int ScrollDif => _apiAccess.GetScrollDif();

		public Input(IGAPIAInput apiAccess)
		{
			_apiAccess = apiAccess;
		}

		/// <summary>
		/// Initializes the input
		/// </summary>
		/// <param name="updater"></param>
		public void Init() => Update(true);

		/// <summary>
		/// Updates the input
		/// </summary>
		public void Update(bool wasFocused)
		{
			if(_apiAccess == null)
				throw new NotInitializedException("Input was not initialized");

			_keyWasPressed = _keyPressed;
			_keyPressed = _apiAccess.UpdateKeys();
			_mouseWasPressed = _mousePressed;
			_mousePressed = _apiAccess.UpdateMouse(wasFocused);
		}

		/// <summary>
		/// Places the cursor in global screen space
		/// </summary>
		/// <param name="loc">The new location that the cursor should be at</param>
		public void PlaceCursor(Point loc) => _apiAccess.PlaceCursor(loc);

		/// <summary>
		/// Whether a keyboard key is being held
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsKeyDown(Key key)
		{
			return _keyPressed.TryGetValue(key, out bool r) ? r : false;
		}

		/// <summary>
		/// Whether a mouse button is being held
		/// </summary>
		/// <param name="btn"></param>
		/// <returns></returns>
		public bool IsKeyDown(MouseButton btn)
		{
			return _mousePressed.TryGetValue(btn, out bool r) ? r : false;
		}

		/// <summary>
		/// Whether a keyboard key is not being held
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsKeyUp(Key key)
		{
			return !IsKeyDown(key);
		}

		/// <summary>
		/// Whether a mouse button is not being held
		/// </summary>
		/// <param name="btn"></param>
		/// <returns></returns>
		public bool IsKeyUp(MouseButton btn)
		{
			return !IsKeyDown(btn);
		}

		/// <summary>
		/// Whether a keyboard key was pressed
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool KeyPressed(Key key)
		{
			return IsKeyDown(key) && !_keyWasPressed[key];
		}

		/// <summary>
		/// Whether a mouse button was pressed
		/// </summary>
		/// <param name="btn"></param>
		/// <returns></returns>
		public bool KeyPressed(MouseButton btn)
		{
			return IsKeyDown(btn) && !_mouseWasPressed[btn];
		}

		/// <summary>
		/// Whether a keyboard key was released
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool KeyReleased(Key key)
		{
			return !IsKeyDown(key) && _keyWasPressed[key];
		}

		/// <summary>
		/// Whether a mouse button was released
		/// </summary>
		/// <param name="btn"></param>
		/// <returns></returns>
		public bool KeyReleased(MouseButton btn)
		{
			return !IsKeyDown(btn) && _mouseWasPressed[btn];
		}
	}
}
