using System;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SonicRetro.SAModel.ModelData.Buffer;
using SonicRetro.SAModel.ModelData;
using Reloaded.Memory.Streams.Writers;
using Reloaded.Memory.Streams;

namespace SonicRetro.SAModel.Graphics.OpenGL.Rendering
{
	public class GLMaterial : RenderMaterial
	{
		/// <summary>
		/// The buffer handle
		/// </summary>
		public static int Handle { get; private set; }

		/// <summary>
		/// Initializes the buffer and sets the handle
		/// </summary>
		unsafe public static void Init()
		{
			Handle = GL.GenBuffer();
			material = new BufferMaterial();
			Buffer();
		}

		public static void Reset()
		{
			material = new BufferMaterial();
		}

		public static void Buffer(BufferMaterial mat)
		{
			material = mat;
			Buffer();
		}

		unsafe public static void Buffer()
		{
			if (material.MaterialFlags.HasFlag(MaterialFlags.useTexture))
			{
				// bind texture here using textureID and the correct texture list
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)material.TextureFiltering.ToGLMinFilter());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)material.TextureFiltering.ToGLMagFilter());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)material.WrapModeU());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)material.WrapModeV());
			}
			if (material.UseAlpha) GL.BlendFunc(material.SourceBlendMode.ToGLBlend(), material.DestinationBlendmode.ToGLBlend());

			if (material.Culling && RenderMode != RenderMode.CullSide)
				GL.Enable(EnableCap.CullFace);
			else GL.Disable(EnableCap.CullFace);

			// buffer the other data
			using (ExtendedMemoryStream stream = new ExtendedMemoryStream(104))
			{
				LittleEndianMemoryStream writer = new LittleEndianMemoryStream(stream);

				ViewPos.Write(writer, Structs.IOType.Float);
				writer.Write(0);

				ViewDir.Write(writer, Structs.IOType.Float);
				writer.Write(0);

				LightDir.Write(writer, Structs.IOType.Float);
				writer.Write(0);

				writer.Write(material.Diffuse.RedF);
				writer.Write(material.Diffuse.GreenF);
				writer.Write(material.Diffuse.BlueF);
				writer.Write(material.Diffuse.AlphaF);

				writer.Write(material.Specular.RedF);
				writer.Write(material.Specular.GreenF);
				writer.Write(material.Specular.BlueF);
				writer.Write(material.Specular.AlphaF);

				writer.Write(material.Ambient.RedF);
				writer.Write(material.Ambient.GreenF);
				writer.Write(material.Ambient.BlueF);
				writer.Write(material.Ambient.AlphaF);

				writer.Write(material.SpecularExponent);
				int flags = (ushort)material.MaterialFlags | ((int)RenderMode << 24);
				writer.Write(flags);

				GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
				fixed (byte* ptr = stream.ToArray())
				{
					GL.BufferData(BufferTarget.UniformBuffer, (int)stream.Length, (IntPtr)ptr, BufferUsageHint.StreamDraw);
				}
				GL.BindBuffer(BufferTarget.UniformBuffer, 0);
			}

		}

	}

	public static class MaterialExtensions
	{
		public static BlendingFactor ToGLBlend(this BlendMode instr)
		{
			switch (instr)
			{
				default:
				case BlendMode.Zero:
					return BlendingFactor.Zero;
				case BlendMode.One:
					return BlendingFactor.One;
				case BlendMode.Other:
					return BlendingFactor.SrcColor;
				case BlendMode.OtherInverted:
					return BlendingFactor.OneMinusSrcColor;
				case BlendMode.SrcAlpha:
					return BlendingFactor.SrcAlpha;
				case BlendMode.SrcAlphaInverted:
					return BlendingFactor.OneMinusSrcAlpha;
				case BlendMode.DstAlpha:
					return BlendingFactor.DstAlpha;
				case BlendMode.DstAlphaInverted:
					return BlendingFactor.OneMinusDstAlpha;
			}
		}

		public static TextureMinFilter ToGLMinFilter(this ModelData.FilterMode filter)
		{
			switch (filter)
			{
				case ModelData.FilterMode.PointSampled:
					return TextureMinFilter.Nearest;
				case ModelData.FilterMode.Bilinear:
				case ModelData.FilterMode.Trilinear:
					return TextureMinFilter.Linear;
				default:
					throw new InvalidCastException($"{filter} has no corresponding OpenGL filter");
			}
		}

		public static TextureMagFilter ToGLMagFilter(this ModelData.FilterMode filter)
		{
			switch (filter)
			{
				case ModelData.FilterMode.PointSampled:
					return TextureMagFilter.Nearest;
				case ModelData.FilterMode.Bilinear:
				case ModelData.FilterMode.Trilinear:
					return TextureMagFilter.Linear;
				default:
					throw new InvalidCastException($"{filter} has no corresponding OpenGL filter");
			}
		}

		public static TextureWrapMode WrapModeU(this BufferMaterial mat)
		{
			if (mat.ClampU)
				return TextureWrapMode.ClampToEdge;
			else return mat.MirrorU ? TextureWrapMode.MirroredRepeat : TextureWrapMode.Repeat;
		}

		public static TextureWrapMode WrapModeV(this BufferMaterial mat)
		{
			if (mat.ClampV)
				return TextureWrapMode.ClampToEdge;
			else return mat.MirrorV ? TextureWrapMode.MirroredRepeat : TextureWrapMode.Repeat;
		}
	}
}
