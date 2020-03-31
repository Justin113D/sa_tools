﻿using Reloaded.Memory.Streams.Writers;
using SonicRetro.SAModel.ModelData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SAModel.ObjData.Animation
{
	/// <summary>
	/// Model and Motion in one struct
	/// </summary>
	public class Action
	{
		/// <summary>
		/// Assigned model
		/// </summary>
		public NjsObject Model { get; }

		/// <summary>
		/// Animation of the model
		/// </summary>
		public Motion Animation { get; }

		/// <summary>
		/// Create a new action
		/// </summary>
		/// <param name="model"></param>
		/// <param name="animation"></param>
		public Action(NjsObject model, Motion animation)
		{
			Model = model;
			Animation = animation;
		}

		/// <summary>
		/// Reads an action from a byte array
		/// </summary>
		/// <param name="source">Byte source</param>
		/// <param name="address">Address at which the action is located</param>
		/// <param name="imagebase">Image base for all addresses</param>
		/// <param name="format">Attach format</param>
		/// <param name="DX">Whether the file is for sadx</param>
		/// <param name="labels">C struct labels</param>
		/// <param name="attaches">Attaches that have already been read</param>
		/// <returns></returns>
		public static Action Read(byte[] source, uint address, uint imagebase, AttachFormat format, bool DX, Dictionary<uint, string> labels, Dictionary<uint, ModelData.Attach> attaches)
		{
			uint mdlAddress = ByteConverter.ToUInt32(source, address);
			if (mdlAddress == 0) throw new FormatException($"Action at {address:X8} does not have a model!");
			mdlAddress -= imagebase;
			NjsObject mdl = NjsObject.Read(source, mdlAddress, imagebase, format, DX, labels, attaches);

			uint aniAddress = ByteConverter.ToUInt32(source, address + 4);
			if(aniAddress == 0) throw new FormatException($"Action at {address:X8} does not have a model!");
			aniAddress -= imagebase;
			Motion mtn = Motion.Read(source, ref aniAddress, imagebase, (uint)mdl.Count(), labels);

			return new Action(mdl, mtn);
		}

		/// <summary>
		/// Writes the action to a stream
		/// </summary>
		/// <param name="writer">Output stream</param>
		/// <param name="imageBase">Image base for all addresses</param>
		/// <param name="DX">Whether the action is for SADX</param>
		/// <param name="labels">C struct labels</param>
		/// <returns>Address to the written action</returns>
		public uint Write(EndianMemoryStream writer, uint imageBase, bool DX, Dictionary<string, uint> labels)
		{
			uint mdlAddress = Model.WriteHierarchy(writer, imageBase, DX, labels);
			uint aniAddress = Animation.Write(writer, imageBase, labels);

			uint address = (uint)writer.Stream.Position + imageBase;
			writer.WriteUInt32(mdlAddress);
			writer.WriteUInt32(aniAddress);
			return address;
		}
	}
}