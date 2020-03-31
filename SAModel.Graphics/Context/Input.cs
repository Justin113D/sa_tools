using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Point = System.Drawing.Point;

namespace SonicRetro.SAModel.Graphics
{
	/// <summary>
	/// Responsible for updating input
	/// </summary>
	public abstract class InputUpdater
	{
		/// <summary>
		/// Current cursor location
		/// </summary>
		public Point CursorLoc;

		/// <summary>
		/// How much the mouse has moved between updates
		/// </summary>
		public Point CursorDif;

		/// <summary>
		/// How much the mouse scroll was moved
		/// </summary>
		public int ScrollDif;


		/// <summary>
		/// Places the cursor in global screen space
		/// </summary>
		/// <param name="loc">The new location that the cursor should be at</param>
		public virtual void PlaceCursor(Point loc)
		{
			CursorLoc = loc;
		}

		/// <summary>
		/// Returns a new set of key-states
		/// </summary>
		/// <returns></returns>
		public abstract Dictionary<Key, bool> UpdateKeys();

		/// <summary>
		/// Returns a new set of mousebutton-states and updates mouse related fields
		/// </summary>
		/// <returns></returns>
		public abstract Dictionary<MouseButton, bool> UpdateMouse(bool wasFocused);
	}

	/// <summary>
	/// Used to receive input from keyboard and mouse
	/// </summary>
	public static class Input
	{
		/// <summary>
		/// Responsible for updating the 
		/// </summary>
		private static InputUpdater _updater;

		/// <summary>
		/// Last state of each key
		/// </summary>
		private static Dictionary<Key, bool> _keyWasPressed;

		/// <summary>
		/// Current state of each key
		/// </summary>
		private static Dictionary<Key, bool> _keyPressed;

		/// <summary>
		/// Last state of each mouse button
		/// </summary>
		private static Dictionary<MouseButton, bool> _mouseWasPressed;

		/// <summary>
		/// Current state of each mouse button
		/// </summary>
		private static Dictionary<MouseButton, bool> _mousePressed;

		/// <summary>
		/// The last read cursor location
		/// </summary>
		public static Point CursorLoc => _updater.CursorLoc;

		/// <summary>
		/// The amount that the cursor moved
		/// </summary>
		public static Point CursorDif => _updater.CursorDif;

		/// <summary>
		/// The amount that the scroll was used 
		/// </summary>
		public static int ScrollDif => _updater.ScrollDif;

		/// <summary>
		/// Initializes the input
		/// </summary>
		/// <param name="updater"></param>
		public static void Init(InputUpdater updater)
		{
			_updater = updater;
			_keyPressed = _updater.UpdateKeys();
			_keyWasPressed = _keyPressed;
			_mousePressed = _updater.UpdateMouse(true);
			_mouseWasPressed = _mousePressed;
		}

		/// <summary>
		/// Updates the input
		/// </summary>
		public static void Update(bool wasFocused)
		{
			if (_updater == null) throw new InvalidOperationException("Input was not initiated");

			_keyWasPressed = _keyPressed;
			_keyPressed = _updater.UpdateKeys();
			_mouseWasPressed = _mousePressed;
			_mousePressed = _updater.UpdateMouse(wasFocused);
		}

		/// <summary>
		/// Places the cursor in global screen space
		/// </summary>
		/// <param name="loc">The new location that the cursor should be at</param>
		public static void PlaceCursor(Point loc)
		{
			_updater.PlaceCursor(loc);
		}

		/// <summary>
		/// Whether a keyboard key is being held
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool IsKeyDown(Key key)
		{
			return _keyPressed.TryGetValue(key, out bool r) ? r : false;
		}

		/// <summary>
		/// Whether a mouse button is being held
		/// </summary>
		/// <param name="btn"></param>
		/// <returns></returns>
		public static bool IsKeyDown(MouseButton btn)
		{
			return _mousePressed.TryGetValue(btn, out bool r) ? r : false;
		}

		/// <summary>
		/// Whether a keyboard key is not being held
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool IsKeyUp(Key key)
		{
			return !IsKeyDown(key);
		}

		/// <summary>
		/// Whether a mouse button is not being held
		/// </summary>
		/// <param name="btn"></param>
		/// <returns></returns>
		public static bool IsKeyUp(MouseButton btn)
		{
			return !IsKeyDown(btn);
		}

		/// <summary>
		/// Whether a keyboard key was pressed
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool KeyPressed(Key key)
		{
			return IsKeyDown(key) && !_keyWasPressed[key];
		}

		/// <summary>
		/// Whether a mouse button was pressed
		/// </summary>
		/// <param name="btn"></param>
		/// <returns></returns>
		public static bool KeyPressed(MouseButton btn)
		{
			return IsKeyDown(btn) && !_mouseWasPressed[btn];
		}

		/// <summary>
		/// Whether a keyboard key was released
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool KeyReleased(Key key)
		{
			return !IsKeyDown(key) && _keyWasPressed[key];
		}

		/// <summary>
		/// Whether a mouse button was released
		/// </summary>
		/// <param name="btn"></param>
		/// <returns></returns>
		public static bool KeyReleased(MouseButton btn)
		{
			return !IsKeyDown(btn) && _mouseWasPressed[btn];
		}
	}
}
