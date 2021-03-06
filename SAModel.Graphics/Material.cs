﻿using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Writers;
using SonicRetro.SAModel.Graphics.APIAccess;
using SonicRetro.SAModel.ModelData.Buffer;
using SonicRetro.SAModel.Structs;
using System;
using System.Collections.ObjectModel;

namespace SonicRetro.SAModel.Graphics
{
	/// <summary>
	/// Buffers and activates a material
	/// </summary>
	public class Material
	{
		/// <summary>
		/// Graphics API Access for buffering Materials
		/// </summary>
		protected readonly IGAPIAMaterial _apiAccess;

		protected BufferMaterial _bufferMaterial;

		/// <summary>
		/// Active material
		/// </summary>
		public BufferMaterial BufferMaterial
		{
			get => _bufferMaterial;
			set
			{
				if(value == null)
					return;
				_bufferMaterial = value.Clone();
				ReBuffer();
			}
		}

		/// <summary>
		/// Viewing position
		/// </summary>
		public Vector3 ViewPos { get; set; }

		/// <summary>
		/// Camera viewing direction
		/// </summary>
		public Vector3 ViewDir { get; set; }

		/// <summary>
		/// Base buffer data
		/// </summary>
		public ReadOnlyCollection<byte> Buffer { get; private set; }

		protected byte[] _buffer;

		public Material(IGAPIAMaterial apiAccess)
		{
			_apiAccess = apiAccess;
			_buffer = new byte[104];
			Buffer = Array.AsReadOnly(_buffer);
			_bufferMaterial = new BufferMaterial();
		}

		/// <summary>
		/// Sets the buffer material to a new instance and buffers it
		/// </summary>
		public void ResetBufferMaterial() => BufferMaterial = new BufferMaterial();

		/// <summary>
		/// Buffers the material into the byte buffer
		/// </summary>
		public virtual void ReBuffer()
		{
			_apiAccess.MaterialPreBuffer(this);

			using(ExtendedMemoryStream stream = new ExtendedMemoryStream(_buffer))
			{
				LittleEndianMemoryStream writer = new LittleEndianMemoryStream(stream);

				ViewPos.Write(writer, IOType.Float);
				writer.Write(0);

				ViewDir.Write(writer, IOType.Float);
				writer.Write(0);

				new Vector3(0, 1, 0).Write(writer, IOType.Float);
				writer.Write(0);

				writer.Write(BufferMaterial.Diffuse.RedF);
				writer.Write(BufferMaterial.Diffuse.GreenF);
				writer.Write(BufferMaterial.Diffuse.BlueF);
				writer.Write(BufferMaterial.Diffuse.AlphaF);

				writer.Write(BufferMaterial.Specular.RedF);
				writer.Write(BufferMaterial.Specular.GreenF);
				writer.Write(BufferMaterial.Specular.BlueF);
				writer.Write(BufferMaterial.Specular.AlphaF);

				writer.Write(BufferMaterial.Ambient.RedF);
				writer.Write(BufferMaterial.Ambient.GreenF);
				writer.Write(BufferMaterial.Ambient.BlueF);
				writer.Write(BufferMaterial.Ambient.AlphaF);

				writer.Write(BufferMaterial.SpecularExponent);
				int flags = (ushort)BufferMaterial.MaterialFlags;
				writer.Write(flags);
			}

			_apiAccess.MaterialPostBuffer(this);
		}
	}
}
