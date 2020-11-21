using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;

namespace SonicRetro.SAModel.Graphics.APIAccess
{
	/// <summary>
	/// Responsible for updating input
	/// </summary>
	public interface IGAPIAInput
	{
		/// <summary>
		/// Returns the current cursor location
		/// </summary>
		Point GetCursorPos();

		/// <summary>
		/// Returns how much the mouse has moved between updates
		/// </summary>
		Point GetCursorDif();

		/// <summary>
		/// Returns how much the scrollwheel was moved
		/// </summary>
		int GetScrollDif();

		/// <summary>
		/// Places the cursor in global screen space
		/// </summary>
		/// <param name="loc">The new location that the cursor should be at</param>
		void PlaceCursor(Point position);

		/// <summary>
		/// Returns a new set of key-states
		/// </summary>
		/// <returns></returns>
		Dictionary<Key, bool> UpdateKeys();

		/// <summary>
		/// Returns a new set of mousebutton-states and updates mouse related fields
		/// </summary>
		Dictionary<MouseButton, bool> UpdateMouse(bool wasFocused);
	}
}
