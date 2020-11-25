using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SATexture
{
	public sealed class TextureSet
	{
		/// <summary>
		/// Textures of this texture set
		/// </summary>
		public ReadOnlyCollection<TextureEntry> Textures { get; }

		public TextureSet(TextureEntry[] textures)
		{
			Textures = Array.AsReadOnly(textures);
		}

		public TextureEntry this[int i]
		{
			get
			{
				return Textures[i];
			}
		}

		
	}
}
