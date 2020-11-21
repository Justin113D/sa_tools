using SonicRetro.SAModel.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SAModel.Graphics.UI
{
	/// <summary>
	/// Text to draw to the UI
	/// </summary>
	public class UIText : UIElement
	{
		/// <summary>
		/// The text to draw
		/// </summary>
		public string Text { get; set; }

		//TODO add font

		public UIText(Vector2 position, Vector2 localPivot, Vector2 globalPivot, float rotation, string text) : base(position, localPivot, globalPivot, rotation)
		{
			Text = text;
		}

		public override object Clone() => MemberwiseClone();
	}
}
