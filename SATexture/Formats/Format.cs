using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SATexture.Formats
{
	/// <summary>
	/// Abstract format class for all texture file formats
	/// </summary>
	public abstract class Format
	{
		/// <summary>
		/// File header for the format
		/// </summary>
		public abstract uint FileHeader { get; }

		/// <summary>
		/// Writes a texture set to a file using the format
		/// </summary>
		/// <param name="path">File path to write to</param>
		/// <param name="set">Texture set to write to</param>
		public abstract void Write(string filename, TextureSet set);

		/// <summary>
		/// Reads a texture set from a file
		/// </summary>
		/// <param name="path">File path</param>
		/// <param name="set">Texture set output</param>
		/// <returns>Whether the file is the format (and can thus be read)</returns>
		public abstract bool Read(string filename, out TextureSet set);
	}
}
