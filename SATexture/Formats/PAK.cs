using SonicRetro.SAModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SATexture.Formats
{
	public sealed class PAK : Format
	{
		private class PAKFile
		{
			public string Name { get; set; }
			public string LongPath { get; set; }
			public byte[] Data { get; set; }

			public PAKFile()
			{
				Name = LongPath = string.Empty;
			}

			public PAKFile(string name, string longpath, byte[] data)
			{
				Name = name;
				LongPath = longpath;
				Data = data;
			}
		}

		public override uint FileHeader => 0x6B617001;

		public override void Write(string filename, TextureSet set)
		{
			throw new NotImplementedException();
		}

		public override bool Read(string filename, out TextureSet set)
		{
			byte[] file = File.ReadAllBytes(filename);

			if(ByteConverter.ToUInt32(file, 0) == FileHeader)
			{
				set = null;
				return false;
			}

			uint fileCount = ByteConverter.ToUInt32(file, 0x39);
			PAKFile[] files = new PAKFile[fileCount];

			uint tmpAddrss = 0x3Du;
			for(int i  = 0; i < fileCount; i++)
			{
				string name = ByteConverter.;

				files[i] = new PAKFile();
			}

			set = null;
			return true;
		}


	}
}
