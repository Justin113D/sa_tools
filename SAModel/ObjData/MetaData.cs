﻿using Reloaded.Memory.Streams.Writers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SAModel.ObjData
{
	/// <summary>
	/// Meta data storage for both mdl and lvl files
	/// </summary>
	[Serializable]
	public class MetaData
	{
		/// <summary>
		/// Author of the file
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Description of the files contents
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// C struct labels (only for reading)
		/// </summary>
		public ReadOnlyDictionary<uint, string> Labels { get; private set; }

		/// <summary>
		/// Animation file paths
		/// </summary>
		public List<string> AnimFiles { get; }

		/// <summary>
		/// Morph file path
		/// </summary>
		public List<string> MorphFiles { get; }

		/// <summary>
		/// Other chunk blocks that have no mappings (for this library)
		/// </summary>
		public Dictionary<uint, byte[]> Other { get; set; }

		/// <summary>
		/// Creates a new empty set of meta data
		/// </summary>
		public MetaData()
		{
			AnimFiles = new List<string>();
			MorphFiles = new List<string>();
			Other = new Dictionary<uint, byte[]>();
			Labels = new ReadOnlyDictionary<uint, string>(new Dictionary<uint, string>());
		}

		/// <summary>
		/// Reads a set of meta data from a byte array
		/// </summary>
		/// <param name="source">Byte source</param>
		/// <param name="version">File version</param>
		/// <param name="mdl">Whether the meta data is coming from an mdl file</param>
		/// <returns></returns>
		public static MetaData Read(byte[] source, int version, bool mdl)
		{
			MetaData result = new MetaData();

			uint tmpAddr = ByteConverter.ToUInt32(source, 0xC);
			Dictionary<uint, string> labels = new Dictionary<uint, string>();
			switch (version)
			{
				case 0:
					if (!mdl) goto case 1;

					// reading animation locations
					if (tmpAddr != 0)
					{
						uint pathAddr = ByteConverter.ToUInt32(source, tmpAddr);
						while (pathAddr != uint.MaxValue)
						{
							result.AnimFiles.Add(source.GetCString(pathAddr));
							tmpAddr += 4;
							pathAddr = ByteConverter.ToUInt32(source, tmpAddr);
						}
					}

					tmpAddr = ByteConverter.ToUInt32(source, 0x10);
					if(tmpAddr != 0)
					{
						uint pathAddr = ByteConverter.ToUInt32(source, tmpAddr);
						while (pathAddr != uint.MaxValue)
						{
							result.MorphFiles.Add(source.GetCString(pathAddr));
							tmpAddr += 4;
							pathAddr = ByteConverter.ToUInt32(source, tmpAddr);
						}
					}

					goto case 1;
				case 1:
					if(mdl) tmpAddr = ByteConverter.ToUInt32(source, 0x14);
					if (tmpAddr == 0) break;

					// version 1 added labels
					uint addr = ByteConverter.ToUInt32(source, tmpAddr);
					while (addr != uint.MaxValue)
					{
						labels.Add(addr, source.GetCString(ByteConverter.ToUInt32(source, tmpAddr + 4)));
						tmpAddr += 8;
						addr = ByteConverter.ToUInt32(source, tmpAddr);
					}
					break;
				case 2:
				case 3:
					// version 2 onwards used "blocks" of meta data, 
					// where version 3 refined the concept to make 
					// a block use local addresses to that block

					if (tmpAddr == 0) break;
					MetaType type = (MetaType)ByteConverter.ToUInt32(source, tmpAddr);

					while(type != MetaType.End)
					{
						uint blockSize = ByteConverter.ToUInt32(source, tmpAddr + 4);
						tmpAddr += 8;
						uint nextMetaBlock = tmpAddr + blockSize;
						uint pathAddr;

						if (version == 2)
						{

							switch (type)
							{
								case MetaType.Label:
									while (ByteConverter.ToInt64(source, tmpAddr) != -1)
									{
										labels.Add(ByteConverter.ToUInt32(source, tmpAddr), source.GetCString(ByteConverter.ToUInt32(source, tmpAddr + 4)));
										tmpAddr += 8;
									}
									break;
								case MetaType.Animation:
									pathAddr = ByteConverter.ToUInt32(source, tmpAddr);
									while (pathAddr != uint.MaxValue)
									{
										result.AnimFiles.Add(source.GetCString(pathAddr));
										tmpAddr += 4;
										pathAddr = ByteConverter.ToUInt32(source, tmpAddr);
									}
									break;
								case MetaType.Morph:
									pathAddr = ByteConverter.ToUInt32(source, tmpAddr);
									while (pathAddr != uint.MaxValue)
									{
										result.MorphFiles.Add(source.GetCString(pathAddr));
										tmpAddr += 4;
										pathAddr = ByteConverter.ToUInt32(source, tmpAddr);
									}
									break;
								case MetaType.Author:
									result.Author = source.GetCString(tmpAddr);
									break;
								case MetaType.Description:
									result.Description = source.GetCString(tmpAddr);
									break;
							}
						}
						else
						{
							byte[] block = new byte[blockSize];
							Array.Copy(source, tmpAddr, block, 0, blockSize);
							uint blockAddr = 0;
							switch (type)
							{
								case MetaType.Label:
									while (ByteConverter.ToInt64(block, blockAddr) != -1)
									{
										labels.Add(ByteConverter.ToUInt32(block, blockAddr),
											block.GetCString(ByteConverter.ToUInt32(block, blockAddr + 4)));
										blockAddr += 8;
									}
									break;
								case MetaType.Animation:
									pathAddr = ByteConverter.ToUInt32(block, blockAddr);
									while (pathAddr != uint.MaxValue)
									{
										result.AnimFiles.Add(block.GetCString(pathAddr));
										blockAddr += 4;
										pathAddr = ByteConverter.ToUInt32(block, blockAddr);
									}
									break;
								case MetaType.Morph:
									pathAddr = ByteConverter.ToUInt32(block, blockAddr);
									while (pathAddr != uint.MaxValue)
									{
										result.MorphFiles.Add(block.GetCString(pathAddr));
										blockAddr += 4;
										pathAddr = ByteConverter.ToUInt32(block, blockAddr);
									}
									break;
								case MetaType.Author:
									result.Author = block.GetCString(0);
									break;
								case MetaType.Description:
									result.Description = block.GetCString(0);
									break;
								default:
									result.Other.Add((uint)type, block);
									break;
							}
						}

						tmpAddr = nextMetaBlock;
						type = (MetaType)ByteConverter.ToUInt32(source, tmpAddr);
					}
					break;
			}
			result.Labels = new ReadOnlyDictionary<uint, string>(labels);

			return result;
		}

		/// <summary>
		/// Writes the meta data to a stream
		/// </summary>
		/// <param name="writer">Output stream</param>
		/// <param name="labels">New set of labels</param>
		public void Write(EndianMemoryStream writer, Dictionary<string, uint> labels)
		{
			// write meta data
			uint metaAddr = (uint)writer.Stream.Position;
			writer.Stream.Seek(0xC, SeekOrigin.Begin);
			writer.WriteUInt32(metaAddr);
			writer.Stream.Seek(0, SeekOrigin.End);

			void MetaHeader(MetaType type, List<byte> metaBytes)
			{
				writer.WriteUInt32((uint)type);
				writer.WriteUInt32((uint)metaBytes.Count);
				writer.Write(metaBytes.ToArray());
			}

			// labels
			if (labels.Count > 0)
			{
				List<byte> meta = new List<byte>((labels.Count * 8) + 8);
				int straddr = (labels.Count * 8) + 8;
				List<byte> strbytes = new List<byte>();
				foreach (KeyValuePair<string, uint> label in labels)
				{
					meta.AddRange(ByteConverter.GetBytes(label.Value));
					meta.AddRange(ByteConverter.GetBytes(straddr + strbytes.Count));
					strbytes.AddRange(Encoding.UTF8.GetBytes(label.Key));
					strbytes.Add(0);
					strbytes.Align(4);
				}
				meta.AddRange(ByteConverter.GetBytes(-1L));
				meta.AddRange(strbytes);
				MetaHeader(MetaType.Label, meta);
			}

			// animation files
			if (AnimFiles != null && AnimFiles.Count > 0)
			{
				List<byte> meta = new List<byte>((AnimFiles.Count + 1) * 4);
				int straddr = (AnimFiles.Count + 1) * 4;
				List<byte> strbytes = new List<byte>();
				for (int i = 0; i < AnimFiles.Count; i++)
				{
					meta.AddRange(ByteConverter.GetBytes(straddr + strbytes.Count));
					strbytes.AddRange(Encoding.UTF8.GetBytes(AnimFiles[i]));
					strbytes.Add(0);
					strbytes.Align(4);
				}
				meta.AddRange(ByteConverter.GetBytes(-1));
				meta.AddRange(strbytes);
				MetaHeader(MetaType.Animation, meta);
			}

			// morph files
			if (MorphFiles != null && MorphFiles.Count > 0)
			{
				List<byte> meta = new List<byte>((MorphFiles.Count + 1) * 4);
				int straddr = (MorphFiles.Count + 1) * 4;
				List<byte> strbytes = new List<byte>();
				for (int i = 0; i < MorphFiles.Count; i++)
				{
					meta.AddRange(ByteConverter.GetBytes(straddr + strbytes.Count));
					strbytes.AddRange(Encoding.UTF8.GetBytes(MorphFiles[i]));
					strbytes.Add(0);
					strbytes.Align(4);
				}
				meta.AddRange(ByteConverter.GetBytes(-1));
				meta.AddRange(strbytes);
				MetaHeader(MetaType.Morph, meta);
			}

			// author
			if (!string.IsNullOrEmpty(Author))
			{
				List<byte> meta = new List<byte>(Author.Length + 1);
				meta.AddRange(Encoding.UTF8.GetBytes(Author));
				meta.Add(0);
				meta.Align(4);
				MetaHeader(MetaType.Author, meta);
			}

			// description
			if (!string.IsNullOrEmpty(Description))
			{
				List<byte> meta = new List<byte>(Description.Length + 1);
				meta.AddRange(Encoding.UTF8.GetBytes(Description));
				meta.Add(0);
				meta.Align(4);
				MetaHeader(MetaType.Description, meta);
			}

			// other metadata
			foreach (var item in Other)
			{
				writer.WriteUInt32(item.Key);
				writer.WriteUInt32((uint)item.Value.Length);
				writer.Write(item.Value);
			}

			writer.WriteUInt32((uint)MetaType.End);
			writer.WriteUInt32(0);
		}
	}
}