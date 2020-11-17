using SonicRetro.SAModel.Graphics.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SonicRetro.SAModel.Graphics.OpenGL.Properties;
using System.Drawing;
using System.Drawing.Imaging;
using SonicRetro.SAModel.Structs;

namespace SonicRetro.SAModel.Graphics.OpenGL
{
	public class GLCanvas : Canvas
	{
		private Shader UIShader;

		/// <summary>
		/// Buffers that can be repurposed
		/// </summary>
		private readonly Queue<(int vaoHandle, int vboHandle, int imgHandle)> _reuse;

		/// <summary>
		/// Buffers that were used in the last cycle
		/// </summary>
		private readonly Dictionary<Guid, (int vaoHandle, int vboHandle, int imgHandle)> _buffers;

		private bool _updateAllVAOs;

		private float _widthFactor;
		private float _heightFactor;

		public GLCanvas() : base()
		{
			_buffers = new Dictionary<Guid, (int vaoHandle, int vboHandle, int imgHandle)>();
			_reuse = new Queue<(int vaoHandle, int vboHandle, int imgHandle)>();
		}

		public void GraphicsInit()
		{
			string vertexShader = Encoding.ASCII.GetString(Resources.DefaultUI_vert).Trim('?');
			string fragShader = Encoding.ASCII.GetString(Resources.DefaultUI_frag).Trim('?');
			UIShader = new Shader(vertexShader, fragShader);
		}

		public override void Render(int width, int height)
		{
			PolygonMode pm = (PolygonMode)GL.GetInteger(GetPName.PolygonMode);

			UIShader.Use();
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			_updateAllVAOs = width != this.width || height != this.height;
			_widthFactor = width * 0.5f;
			_heightFactor = height * 0.5f;
			base.Render(width, height);

			GL.Disable(EnableCap.Blend);
			GL.PolygonMode(MaterialFace.FrontAndBack, pm);

			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		protected override void DrawImage(UIImage image, UIImage old)
		{
			RenderUIElement(image, image.Texture, !image.EqualTransform(old), !image.Texture.Equals(old?.Texture));
		}

		protected override void DrawText(UIText text, UIText old)
		{
			throw new NotImplementedException("TODO: Generate text image"); // TODO
			Bitmap texture = null;
			RenderUIElement(text, texture, !text.EqualTransform(old), text.Text != old.Text);
		}

		private void RenderUIElement(UIElement element, Bitmap image, bool updateTransforms, bool updateImage)
		{
			(int vaoHandle, int vboHandle, int imgHandle) handles;
			if(!_buffers.TryGetValue(element.ID, out handles))
			{
				if(_reuse.Count == 0)
					handles = GenerateTexBuffers();
				else
				{
					handles = _reuse.Dequeue();
					GL.BindVertexArray(handles.vaoHandle);
					GL.BindBuffer(BufferTarget.ArrayBuffer, handles.vboHandle);
					GL.BindTexture(TextureTarget.Texture2D, handles.imgHandle);
				}
				_buffers.Add(element.ID, handles);
			}
			else
			{
				GL.BindVertexArray(handles.vaoHandle);
				GL.BindBuffer(BufferTarget.ArrayBuffer, handles.vboHandle);
				GL.BindTexture(TextureTarget.Texture2D, handles.imgHandle);
			}

			if(updateTransforms || updateImage)
				UpdateVAO(element.Position, element.LocalPivot, element.GlobalPivot, new Vector2(image.Width, image.Height), element.Rotation);
			if(updateImage)
				UpdateTexture(image);

			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
		}

		private (int vaoHandle, int vboHandle, int imgHandle) GetHandles(Guid id)
		{
			(int vaoHandle, int vboHandle, int imgHandle) handles;
			if(!_buffers.TryGetValue(id, out handles))
			{
				if(_reuse.Count == 0)
					handles = GenerateTexBuffers();
				else
				{
					handles = _reuse.Dequeue();
					GL.BindVertexArray(handles.vaoHandle);
					GL.BindTexture(TextureTarget.Texture2D, handles.imgHandle);
				}
			}
			else
			{
				GL.BindVertexArray(handles.vaoHandle);
				GL.BindTexture(TextureTarget.Texture2D, handles.imgHandle);
			}

			return handles;
		}

		private (int vaoHandle, int vboHandle, int imgHandle) GenerateTexBuffers()
		{
			int vaoHandle = GL.GenVertexArray();
			int vboHandle = GL.GenBuffer();
			GL.BindVertexArray(vaoHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);

			// assigning attribute data
			// position
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 16, 0);

			// uv
			GL.EnableVertexAttribArray(3);
			GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 16, 8);

			int imgHandle = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, imgHandle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

			return (vaoHandle, vboHandle, imgHandle);
		}

		unsafe private void UpdateVAO(Vector2 position, Vector2 localPivot, Vector2 globalPivot, Vector2 dimensions, float rotation)
		{
			// TODO rotation
			float left =   (position.X - dimensions.X * localPivot.X + width * globalPivot.X) / _widthFactor - 1;
			float bottom = (position.Y - dimensions.Y * localPivot.Y + height * globalPivot.Y) / _heightFactor - 1;
			float right = left + (dimensions.X / width * 2);
			float top = bottom + (dimensions.Y / height * 2);

			// uv locations always stay the same
			float[] posUV = new float[] {   left,  bottom, 0, 1,
											right, bottom, 1, 1,
											left,  top,    0, 0,
											right, top,    1, 0 };

			fixed(float* ptr = posUV)
			{
				GL.BufferData(BufferTarget.ArrayBuffer, 64, (IntPtr)ptr, BufferUsageHint.DynamicDraw);
			}
		}

		private void UpdateTexture(Bitmap texture)
		{
			BitmapData data = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			texture.UnlockBits(data);
		}
	}
}
