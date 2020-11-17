using SonicRetro.SAModel.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SAModel.Graphics.UI
{
	/// <summary>
	/// Renders an Image to the UI
	/// </summary>
	public class UIImage : UIElement
	{
		/// <summary>
		/// Texture to draw
		/// </summary>
		public Bitmap Texture { get; set; }

		public UIImage(Vector2 position, Vector2 localPivot, Vector2 globalPivot, float rotation, Bitmap texture) : base(position, localPivot, globalPivot, rotation)
		{
			Texture = texture;
		}

		public override object Clone()
		{
			UIImage clone = (UIImage)base.Clone();
			clone.Texture = (Bitmap)Texture?.Clone();
			return clone;
		}
	}
}
