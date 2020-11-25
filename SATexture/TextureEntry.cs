using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SATexture
{
	/// <summary>
	/// A single texture in a texture set
	/// </summary>
    public sealed class TextureEntry
    {
		public TextureSet ParentSet { get; private set; }

		/// <summary>
		/// Image texture
		/// </summary>
		public Bitmap Texture { get; set; }

		/// <summary>
		/// Name of the texture
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Full path to the file
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Index in the TextureSet
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// Global index of the texture
		/// </summary>
		public int GlobalIndex { get; set; }

		///// <summary>
		///// Texture scaling to use
		///// </summary>
		//public int OverrideWidth { get; set; }

		///// <summary>
		///// Texture scaling to use
		///// </summary>
		//public int OverrideHeight { get; set; }
    }
}
