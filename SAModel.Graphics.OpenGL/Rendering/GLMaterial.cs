using System;
using OpenTK.Graphics.OpenGL4;
using SonicRetro.SAModel.ModelData.Buffer;
using SonicRetro.SAModel.ModelData;
using Reloaded.Memory.Streams.Writers;
using Reloaded.Memory.Streams;

namespace SonicRetro.SAModel.Graphics.OpenGL.Rendering
{
	public class GLMaterial
	{
		/// <summary>
		/// Active material
		/// </summary>
		public static BufferMaterial Material { get; private set; }

		/// <summary>
		/// Global rendering mode
		/// </summary>
		public static RenderMode RenderMode { get; set; }

		/// <summary>
		/// Viewing position
		/// </summary>
		public static Structs.Vector3 ViewPos { get; set; }

		/// <summary>
		/// Camera viewing direction
		/// </summary>
		public static Structs.Vector3 ViewDir { get; set; }

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
			Material = new BufferMaterial();
			ReBuffer();
		}

		public static void Reset()
		{
			Material = new BufferMaterial();
		}

		public static void Buffer(BufferMaterial mat)
		{
			Material = mat;
			ReBuffer();
		}

		unsafe public static void ReBuffer()
		{
			if (Material.MaterialFlags.HasFlag(MaterialFlags.useTexture))
			{
				// bind texture here using textureID and the correct texture list
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Material.TextureFiltering.ToGLMinFilter());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Material.TextureFiltering.ToGLMagFilter());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Material.WrapModeU());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Material.WrapModeV());
			}
			if (Material.UseAlpha) GL.BlendFunc(Material.SourceBlendMode.ToGLBlend(), Material.DestinationBlendmode.ToGLBlend());

			if (Material.Culling && RenderMode != RenderMode.CullSide)
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

				new Structs.Vector3(0, 1, 0).Write(writer, Structs.IOType.Float);
				writer.Write(0);

				writer.Write(Material.Diffuse.RedF);
				writer.Write(Material.Diffuse.GreenF);
				writer.Write(Material.Diffuse.BlueF);
				writer.Write(Material.Diffuse.AlphaF);

				writer.Write(Material.Specular.RedF);
				writer.Write(Material.Specular.GreenF);
				writer.Write(Material.Specular.BlueF);
				writer.Write(Material.Specular.AlphaF);

				writer.Write(Material.Ambient.RedF);
				writer.Write(Material.Ambient.GreenF);
				writer.Write(Material.Ambient.BlueF);
				writer.Write(Material.Ambient.AlphaF);

				writer.Write(Material.SpecularExponent);
				int flags = (ushort)Material.MaterialFlags | ((int)RenderMode << 24);
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
