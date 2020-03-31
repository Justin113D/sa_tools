using SonicRetro.SAModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SA_Tools.SplitMDL
{
	public static class SplitMDL
	{
		public static void Split(bool? isBigEndian, string filePath, string outputFolder, string[] animationPaths)
		{
			string dir = Environment.CurrentDirectory;
			try
			{
				if (outputFolder[outputFolder.Length - 1] != '/') outputFolder = string.Concat(outputFolder, "/");

				// get file name, read it from the console if nothing
				string mdlfilename = filePath;

				mdlfilename = Path.GetFullPath(mdlfilename);

				// load model file
				byte[] mdlfile = File.ReadAllBytes(mdlfilename);
				if (Path.GetExtension(mdlfilename).Equals(".prs", StringComparison.OrdinalIgnoreCase))
					mdlfile = FraGag.Compression.Prs.Decompress(mdlfile);
				switch (isBigEndian)
				{
					case true:
						ByteConverter.BigEndian = true;
						break;
					case false:
						ByteConverter.BigEndian = false;
						break;
					case null:
						ByteConverter.BigEndian = false;
						uint addr = 0;
						short ile = ByteConverter.ToInt16(mdlfile, 0);
						if (ile == 0)
						{
							ile = ByteConverter.ToInt16(mdlfile, 8);
							addr = 8;
						}
						ByteConverter.BigEndian = true;
						if (ile < ByteConverter.ToInt16(mdlfile, addr))
							ByteConverter.BigEndian = false;
						break;
				}
				Environment.CurrentDirectory = Path.GetDirectoryName(mdlfilename);
				(string filename, byte[] data)[] animfiles = new (string, byte[])[animationPaths.Length];
				for (int j = 0; j < animationPaths.Length; j++)
				{
					byte[] data = File.ReadAllBytes(animationPaths[j]);
					if (Path.GetExtension(animationPaths[j]).Equals(".prs", StringComparison.OrdinalIgnoreCase))
						data = FraGag.Compression.Prs.Decompress(data);
					animfiles[j] = (Path.GetFileNameWithoutExtension(animationPaths[j]), data);
				}
				Environment.CurrentDirectory = (outputFolder.Length != 0) ? outputFolder : Path.GetDirectoryName(mdlfilename);
				Directory.CreateDirectory(Path.GetFileNameWithoutExtension(mdlfilename));

				// getting model pointers
				uint address = 0;
				uint i = ByteConverter.ToUInt32(mdlfile, address);
				SortedDictionary<uint, uint> modeladdrs = new SortedDictionary<uint, uint>();
				while (i != uint.MaxValue)
				{
					modeladdrs[i] = ByteConverter.ToUInt32(mdlfile, address + 4);
					address += 8;
					i = ByteConverter.ToUInt32(mdlfile, address);
				}

				// load models from pointer list
				Dictionary<uint, NJS_OBJECT> models = new Dictionary<uint, NJS_OBJECT>();
				Dictionary<uint, string> modelnames = new Dictionary<uint, string>();
				List<string> partnames = new List<string>();
				foreach (KeyValuePair<uint, uint> item in modeladdrs)
				{
					NJS_OBJECT obj = new NJS_OBJECT(mdlfile, item.Value, 0, ModelFormat.Chunk, new Dictionary<uint, Attach>());
					modelnames[item.Key] = obj.Name;
					if (!partnames.Contains(obj.Name))
					{
						List<string> names = new List<string>(obj.GetObjects().Select((o) => o.Name));
						foreach (uint idx in modelnames.Where(a => names.Contains(a.Value)).Select(a => a.Key))
							models.Remove(idx);
						models[item.Key] = obj;
						partnames.AddRange(names);
					}
				}

				// load animations
				Dictionary<uint, string> animfns = new Dictionary<uint, string>();
				Dictionary<uint, NJS_MOTION> anims = new Dictionary<uint, NJS_MOTION>();
				foreach ((string anifilename, byte[] anifile) in animfiles)
				{
					Dictionary<uint, uint> processedanims = new Dictionary<uint, uint>();
					MTNInfo ini = new MTNInfo() { BigEndian = ByteConverter.BigEndian };
					Directory.CreateDirectory(anifilename);
					address = 0;
					i = ByteConverter.ToUInt16(anifile, address);
					while (i != ushort.MaxValue)
					{
						uint aniaddr = ByteConverter.ToUInt32(anifile, address + 4);
						if (!processedanims.ContainsKey(aniaddr))
						{
							anims[i] = new NJS_MOTION(anifile, ByteConverter.ToUInt32(anifile, address + 4), 0, ByteConverter.ToInt16(anifile, address + 2));
							animfns[i] = Path.Combine(anifilename, i.ToString(NumberFormatInfo.InvariantInfo) + ".saanim");
							anims[i].Save(animfns[i]);
							processedanims[aniaddr] = i;
						}
						ini.Indexes[(ushort)i] = "animation_" + aniaddr.ToString("X8");
						address += 8;
						i = ByteConverter.ToUInt16(anifile, address);
					}
					IniSerializer.Serialize(ini, Path.Combine(anifilename, anifilename + ".ini"));
				}

				// save output model files
				foreach (KeyValuePair<uint, NJS_OBJECT> model in models)
				{
					List<string> animlist = new List<string>();
					foreach (KeyValuePair<uint, NJS_MOTION> anim in anims)
						if (model.Value.CountAnimated() == anim.Value.ModelParts)
						{
							string rel = animfns[anim.Key].Replace(outputFolder, string.Empty);
							if (rel.Length > 1 && rel[1] != ':') rel = "../" + rel;
							animlist.Add(rel);
						}

					ModelFile.CreateFile(Path.Combine(Path.GetFileNameWithoutExtension(mdlfilename),
						model.Key.ToString(NumberFormatInfo.InvariantInfo) + ".sa2mdl"), model.Value, animlist.ToArray(),
						null, null, null, ModelFormat.Chunk);
				}

				// save ini file
				IniSerializer.Serialize(new MDLInfo() { BigEndian = ByteConverter.BigEndian, Indexes = modelnames },
					Path.Combine(Path.GetFileNameWithoutExtension(mdlfilename), Path.GetFileNameWithoutExtension(mdlfilename) + ".ini"));
			}
			finally
			{
				Environment.CurrentDirectory = dir;
			}
		}
	}

	public class MDLInfo
	{
		public bool BigEndian { get; set; }
		[IniCollection(IniCollectionMode.IndexOnly)]
		public Dictionary<uint, string> Indexes { get; set; } = new Dictionary<uint, string>();
	}

	public class MTNInfo
	{
		public bool BigEndian { get; set; }
		[IniCollection(IniCollectionMode.IndexOnly)]
		public Dictionary<ushort, string> Indexes { get; set; } = new Dictionary<ushort, string>();
	}
}
