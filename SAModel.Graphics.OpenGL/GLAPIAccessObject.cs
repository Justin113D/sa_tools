using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK.Platform;
using OpenTK.Wpf;
using SonicRetro.SAModel.Graphics.APIAccess;
using SonicRetro.SAModel.Graphics.OpenGL.Properties;
using SonicRetro.SAModel.ModelData.Buffer;
using SonicRetro.SAModel.ObjData;
using SonicRetro.SAModel.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Key = System.Windows.Input.Key;
using Point = System.Drawing.Point;
using TKey = OpenTK.Input.Key;
using UIElement = SonicRetro.SAModel.Graphics.UI.UIElement;

namespace SonicRetro.SAModel.Graphics.OpenGL
{
	/// <summary>
	/// GL API Access object
	/// </summary>
	public sealed class GLAPIAccessObject : GAPIAccessObject
	{
		private Shader _defaultShader;
		//private Shader _debugShader;

		public override void GraphicsInit(Context context)
		{
			GL.Viewport(default, context.Resolution);
			GL.ClearColor(context.BackgroundColor.SystemCol);
			GL.Enable(EnableCap.DepthTest);
			GL.Uniform1(13, 0f); // setting normal offset for wireframe

			// Material
			_materialHandle = GL.GenBuffer();

			// loading the shader
			string vertexShader = Encoding.ASCII.GetString(Resources.VertexShader).Trim('?');
			string fragShader = Encoding.ASCII.GetString(Resources.FragShader).Trim('?');
			_defaultShader = new Shader(vertexShader, fragShader);
			_defaultShader.BindUniformBlock("Material", 0, _materialHandle);

			// canvas
			vertexShader = Encoding.ASCII.GetString(Resources.DefaultUI_vert).Trim('?');
			fragShader = Encoding.ASCII.GetString(Resources.DefaultUI_frag).Trim('?');
			_uiShader = new Shader(vertexShader, fragShader);

			_reuse = new Queue<UIBuffer>();
			_buffers = new Dictionary<Guid, UIBuffer>();

			// for debug
			if(context.GetType() == typeof(DebugContext))
			{
				((DebugContext)context).SphereMesh.Buffer(null, false);
			}

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		public override void AsWindow(Context context)
		{
			var res = context.Resolution;
			var wnd = new GLWindow(context, res.Width, res.Height);
			context.Location = wnd.Location;
			wnd.Run(60, 60);
		}

		public override FrameworkElement AsControl(Context context, HwndSource windowSource)
		{
			GLWpfControl control = new GLWpfControl();
			control.Ready += context.GraphicsInit;

			control.Render += (time) =>
			{
				context.Update(time.TotalSeconds);
				context.Render();
			};

			IWindowInfo window = Utilities.CreateWindowsWindowInfo(windowSource.Handle);
			GraphicsContext grContext = new GraphicsContext(new GraphicsMode(ColorFormat.Empty, 24, 0, 4), window);

			grContext.LoadAll();
			grContext.MakeCurrent(window);

			GLWpfControlSettings settings = new GLWpfControlSettings()
			{
				ContextToUse = grContext,
				GraphicsContextFlags = GraphicsContextFlags.ForwardCompatible,
				MajorVersion = 4,
				MinorVersion = 0,
			};

			control.Start(settings);
			return control;
		}

		public override void UpdateViewport(Rectangle screen, bool resized)
		{
			if(resized)
			{
				GL.Viewport(screen.Size);
			}
		}

		public override void UpdateBackgroundColor(Structs.Color color)
		{
			GL.ClearColor(color.SystemCol);
		}

		public override void DebugUpdateWireframe(WireFrameMode wireframeMode)
		{
			switch(wireframeMode)
			{
				case WireFrameMode.None:
				case WireFrameMode.Overlay:
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
					break;
				case WireFrameMode.ReplaceLine:
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
					break;
				case WireFrameMode.ReplacePoint:
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
					break;
				default:
					break;
			}
		}

		public override void DebugUpdateBoundsMode(BoundsMode boundsMode) { }

		public override void DebugUpdateRenderMode(RenderMode renderMode) { }

		public override void Render(Context context)
		{
			context.Material.ViewPos = context.Camera.Realposition;
			context.Material.ViewDir = context.Camera.Orthographic ? context.Camera.ViewDir : default;

			RenderExtensions.ClearWeights();

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			List<GLRenderMesh> renderMeshes = new List<GLRenderMesh>();
			List<LandEntry> entries = new List<LandEntry>();

			foreach(LandEntry le in context.Scene.VisualGeometry)
				le.Prepare(renderMeshes, entries, context.Camera, _cameraViewMatrix, _cameraProjectionmatrix, null);
			foreach(GameTask tsk in context.Scene.objects)
			{
				tsk.Display();
				tsk.obj.Prepare(renderMeshes, _cameraViewMatrix, _cameraProjectionmatrix, null, null, tsk.obj.HasWeight);
			}

			_defaultShader.Use();

			// first the opaque meshes
			RenderExtensions.RenderModels(renderMeshes, false, context.Material);

			// then transparent meshes
			GL.Enable(EnableCap.Blend);
			RenderExtensions.RenderModels(renderMeshes, true, context.Material);
			GL.Disable(EnableCap.Blend);
		}

		public override uint RenderDebug(DebugContext context)
		{
			context.Material.ViewPos = context.Camera.Realposition;
			context.Material.ViewDir = context.Camera.Orthographic ? context.Camera.ViewDir : default;
			context.Material.RenderMode = context.RenderMode;

			RenderExtensions.ClearWeights();

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			List<GLRenderMesh> renderMeshes = new List<GLRenderMesh>();
			List<LandEntry> entries = new List<LandEntry>();

			if(!context.RenderCollision)
			{
				foreach(LandEntry le in context.Scene.VisualGeometry)
					le.Prepare(renderMeshes, entries, context.Camera, _cameraViewMatrix, _cameraProjectionmatrix, context.ActiveLE);
				foreach(GameTask tsk in context.Scene.objects)
				{
					tsk.Display();
					tsk.obj.Prepare(renderMeshes, _cameraViewMatrix, _cameraProjectionmatrix, context.ActiveNJO, null, tsk.obj.HasWeight);
				}
			}
			else
				foreach(LandEntry le in context.Scene.CollisionGeometry)
					le.Prepare(renderMeshes, entries, context.Camera, _cameraViewMatrix, _cameraProjectionmatrix, context.ActiveLE);

			_defaultShader.Use();
			if(context.RenderCollision)
				context.Material.RenderMode = RenderMode.FullBright;

			// first the opaque meshes
			RenderExtensions.RenderModels(renderMeshes, false, context.Material);
			if(context.WireframeMode == WireFrameMode.Overlay)
				RenderExtensions.RenderModelsWireframe(renderMeshes, false, context.Material);

			// then transparent meshes
			GL.Enable(EnableCap.Blend);

			// then the transparent meshes
			RenderExtensions.RenderModels(renderMeshes, true, context.Material);

			if(context.WireframeMode == WireFrameMode.Overlay)
				RenderExtensions.RenderModelsWireframe(renderMeshes, true, context.Material);

			if(context.BoundsMode != BoundsMode.None && context.ActiveLE != null)
			{
				GL.Disable(EnableCap.DepthTest);
				context.Material.RenderMode = RenderMode.Falloff;
				Matrix4 normal = Matrix4.Identity;
				GL.UniformMatrix4(11, false, ref normal);

				List<LandEntry> boundObjs;

				if(context.BoundsMode == BoundsMode.All)
				{
					boundObjs = context.Scene.geometry;
				}
				else
				{
					boundObjs = new List<LandEntry>
					{
						context.ActiveLE
					};
				}

				foreach(LandEntry le in boundObjs)
				{
					Bounds b = le.ModelBounds;
					Matrix4 world = Matrix4.CreateScale(b.Radius) * Matrix4.CreateTranslation(b.Position.ToGL());
					GL.UniformMatrix4(10, false, ref world);
					world = world * _cameraViewMatrix * _cameraProjectionmatrix;
					GL.UniformMatrix4(12, false, ref world);
					context.SphereMesh.Render(null, true, false, context.Material);
				}

				GL.Enable(EnableCap.DepthTest);
			}
			GL.Disable(EnableCap.Blend);

			return (uint)renderMeshes.Count;
		}

		#region Input

		/// <summary>
		/// Key mapper to convert from OpenTK input to API input
		/// </summary>
		private static readonly Dictionary<Key, TKey> Keymap = new Dictionary<Key, TKey>()
		{
			{ Key.None , TKey.Unknown },
			{ Key.LeftShift , TKey.LShift },
			{ Key.RightShift , TKey.RShift },
			{ Key.LeftCtrl , TKey.LControl },
			{ Key.RightCtrl , TKey.RControl },
			{ Key.LeftAlt , TKey.LAlt },
			{ Key.RightAlt , TKey.RAlt },
			{ Key.LWin , TKey.LWin },
			{ Key.RWin , TKey.RWin },
			{ Key.F1 , TKey.F1 },
			{ Key.F2 , TKey.F2 },
			{ Key.F3 , TKey.F3 },
			{ Key.F4 , TKey.F4 },
			{ Key.F5 , TKey.F5 },
			{ Key.F6 , TKey.F6 },
			{ Key.F7 , TKey.F7 },
			{ Key.F8 , TKey.F8 },
			{ Key.F9 , TKey.F9 },
			{ Key.F10 , TKey.F10 },
			{ Key.F11 , TKey.F11 },
			{ Key.F12 , TKey.F12 },
			{ Key.F13 , TKey.F13 },
			{ Key.F14 , TKey.F14 },
			{ Key.F15 , TKey.F15 },
			{ Key.F16 , TKey.F16 },
			{ Key.F17 , TKey.F17 },
			{ Key.F18 , TKey.F18 },
			{ Key.F19 , TKey.F19 },
			{ Key.F20 , TKey.F20 },
			{ Key.F21 , TKey.F21 },
			{ Key.F22 , TKey.F22 },
			{ Key.F23 , TKey.F23 },
			{ Key.F24 , TKey.F24 },
			{ Key.Up , TKey.Up },
			{ Key.Left , TKey.Left },
			{ Key.Right , TKey.Right },
			{ Key.Down , TKey.Down },
			{ Key.Enter, TKey.Enter },
			{ Key.Escape , TKey.Escape },
			{ Key.Space , TKey.Space },
			{ Key.Tab , TKey.Tab },
			{ Key.Back , TKey.Back },
			{ Key.Insert , TKey.Insert },
			{ Key.Delete , TKey.Delete },
			{ Key.PageUp , TKey.PageUp },
			{ Key.PageDown , TKey.PageDown },
			{ Key.Home , TKey.Home },
			{ Key.End , TKey.End },
			{ Key.CapsLock , TKey.CapsLock },
			{ Key.Scroll , TKey.ScrollLock },
			{ Key.PrintScreen , TKey.PrintScreen },
			{ Key.Pause , TKey.Pause },
			{ Key.NumLock , TKey.NumLock },
			{ Key.Clear , TKey.Clear },
			{ Key.Sleep , TKey.Sleep },
			{ Key.NumPad0 , TKey.Keypad0 },
			{ Key.NumPad1 , TKey.Keypad1 },
			{ Key.NumPad2 , TKey.Keypad2 },
			{ Key.NumPad3 , TKey.Keypad3 },
			{ Key.NumPad4 , TKey.Keypad4 },
			{ Key.NumPad5 , TKey.Keypad5 },
			{ Key.NumPad6 , TKey.Keypad6 },
			{ Key.NumPad7 , TKey.Keypad7 },
			{ Key.NumPad8 , TKey.Keypad8 },
			{ Key.NumPad9 , TKey.Keypad9 },
			{ Key.Divide , TKey.KeypadDivide },
			{ Key.Multiply , TKey.KeypadMultiply },
			{ Key.Subtract , TKey.KeypadSubtract },
			{ Key.Add , TKey.KeypadAdd },
			{ Key.Decimal , TKey.KeypadDecimal },
			{ Key.A , TKey.A },
			{ Key.B , TKey.B },
			{ Key.C , TKey.C },
			{ Key.D , TKey.D },
			{ Key.E , TKey.E },
			{ Key.F , TKey.F },
			{ Key.G , TKey.G },
			{ Key.H , TKey.H },
			{ Key.I , TKey.I },
			{ Key.J , TKey.J },
			{ Key.K , TKey.K },
			{ Key.L , TKey.L },
			{ Key.M , TKey.M },
			{ Key.N , TKey.N },
			{ Key.O , TKey.O },
			{ Key.P , TKey.P },
			{ Key.Q , TKey.Q },
			{ Key.R , TKey.R },
			{ Key.S , TKey.S },
			{ Key.T , TKey.T },
			{ Key.U , TKey.U },
			{ Key.V , TKey.V },
			{ Key.W , TKey.W },
			{ Key.X , TKey.X },
			{ Key.Y , TKey.Y },
			{ Key.Z , TKey.Z },
			{ Key.D0 , TKey.Number0 },
			{ Key.D1 , TKey.Number1 },
			{ Key.D2 , TKey.Number2 },
			{ Key.D3 , TKey.Number3 },
			{ Key.D4 , TKey.Number4 },
			{ Key.D5 , TKey.Number5 },
			{ Key.D6 , TKey.Number6 },
			{ Key.D7 , TKey.Number7 },
			{ Key.D8 , TKey.Number8 },
			{ Key.D9 , TKey.Number9 },
			{ Key.OemTilde , TKey.Tilde },
			{ Key.OemMinus , TKey.Minus },
			{ Key.OemPlus , TKey.Plus },
			{ Key.OemOpenBrackets , TKey.BracketLeft },
			{ Key.OemCloseBrackets , TKey.BracketRight },
			{ Key.OemSemicolon , TKey.Semicolon },
			{ Key.OemQuotes , TKey.Quote },
			{ Key.OemComma , TKey.Comma },
			{ Key.OemPeriod , TKey.Period },
			{ Key.OemQuestion , TKey.Slash },
			{ Key.OemBackslash , TKey.BackSlash }
		};

		private Point _cursorPos;

		private Point _cursorDif;

		private int _scrollDif;
		private int _lastScroll;

		public override Point GetCursorPos() => _cursorPos;

		public override Point GetCursorDif() => _cursorDif;

		public override int GetScrollDif() => _scrollDif;


		public override void PlaceCursor(Point position)
		{
			OpenTK.Input.Mouse.SetPosition(position.X, position.Y);
			_cursorPos = position;
		}

		public override Dictionary<Key, bool> UpdateKeys()
		{
			Dictionary<Key, bool> dict = new Dictionary<Key, bool>();
			KeyboardState state = OpenTK.Input.Keyboard.GetState();
			foreach(Key k in Keymap.Keys)
			{
				dict.Add(k, state.IsKeyDown(Keymap[k]));
			}
			return dict;
		}

		public override Dictionary<MouseButton, bool> UpdateMouse(bool wasFocused)
		{
			Dictionary<MouseButton, bool> dict = new Dictionary<MouseButton, bool>();
			MouseState state = OpenTK.Input.Mouse.GetCursorState();
			for(int i = 0; i < 5; i++)
			{
				dict.Add((MouseButton)i, state.IsButtonDown((OpenTK.Input.MouseButton)i));
			}

			Point mousePos = new Point(state.X, state.Y);
			_cursorDif = wasFocused ? new Point(mousePos.X - _cursorPos.X, mousePos.Y - _cursorPos.Y) : default;
			_cursorPos = mousePos;

			_scrollDif = _lastScroll - state.ScrollWheelValue;
			_lastScroll = state.ScrollWheelValue;

			return dict;
		}

		#endregion

		#region Material

		private int _materialHandle;

		public override void MaterialPreBuffer(Material material)
		{

		}

		unsafe public override void MaterialPostBuffer(Material material)
		{
			if(material.BufferMaterial.MaterialFlags.HasFlag(MaterialFlags.useTexture))
			{
				/* TODO bind texture here using textureID and the correct texture list
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)material.BufferMaterial.TextureFiltering.ToGLMinFilter());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)material.BufferMaterial.TextureFiltering.ToGLMagFilter());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)material.BufferMaterial.WrapModeU());
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)material.BufferMaterial.WrapModeV());
				*/
			}

			if(material.BufferMaterial.UseAlpha)
				GL.BlendFunc(material.BufferMaterial.SourceBlendMode.ToGLBlend(), material.BufferMaterial.DestinationBlendmode.ToGLBlend());

			if(material.BufferMaterial.Culling)// && RenderMode != RenderMode.CullSide)
				GL.Enable(EnableCap.CullFace);
			else
				GL.Disable(EnableCap.CullFace);

			GL.BindBuffer(BufferTarget.UniformBuffer, _materialHandle);
			fixed(byte* ptr = material.Buffer.ToArray())
			{
				GL.BufferData(BufferTarget.UniformBuffer, material.Buffer.Count, (IntPtr)ptr, BufferUsageHint.StreamDraw);
			}
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}

		#endregion

		#region Camera

		private Matrix4 _cameraViewMatrix;
		private Matrix4 _cameraProjectionmatrix;

		private Matrix4 CreateRotationMatrix(Structs.Vector3 rotation)
		{
			return Matrix4.CreateRotationZ(OpenTK.MathHelper.DegreesToRadians(rotation.Z)) *
					Matrix4.CreateRotationY(OpenTK.MathHelper.DegreesToRadians(rotation.Y)) *
					Matrix4.CreateRotationX(OpenTK.MathHelper.DegreesToRadians(rotation.X));
		}

		public override void SetOrtographicMatrix(float width, float height, float zNear, float zFar)
		{
			_cameraProjectionmatrix = Matrix4.CreateOrthographic(width, height, zNear, zFar);
		}

		public override void SetPerspectiveMatrix(float fovy, float aspect, float zNear, float zFar)
		{
			_cameraProjectionmatrix = Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, zNear, zFar);
		}

		public override void UpdateDirections(Structs.Vector3 rotation, out Structs.Vector3 up, out Structs.Vector3 forward, out Structs.Vector3 right)
		{
			Matrix4 mtx = CreateRotationMatrix(rotation);
			forward = new OpenTK.Vector3(mtx * -Vector4.UnitZ).ToSA().Normalized();
			up = new OpenTK.Vector3(mtx * Vector4.UnitY).ToSA().Normalized();
			right = new OpenTK.Vector3(mtx * -Vector4.UnitX).ToSA().Normalized();
		}

		public override Structs.Vector3 ToViewPos(Structs.Vector3 position)
		{
			Vector4 viewPos = (position.ToGL4() * _cameraViewMatrix);
			return new Structs.Vector3(viewPos.X, viewPos.Y, viewPos.Z);
		}

		private Matrix4 GetViewMatrix(Structs.Vector3 position, Structs.Vector3 rotation)
		{
			return Matrix4.CreateTranslation(-position.ToGL()) * CreateRotationMatrix(rotation);
		}

		public override void SetViewMatrix(Structs.Vector3 position, Structs.Vector3 rotation)
		{
			_cameraViewMatrix = GetViewMatrix(position, rotation);
		}

		public override void SetOrbitViewMatrix(Structs.Vector3 position, Structs.Vector3 rotation, Structs.Vector3 orbitOffset)
		{
			_cameraViewMatrix = Matrix4.CreateTranslation(orbitOffset.ToGL()) * GetViewMatrix(position, rotation);
		}

		#endregion

		#region Canvas

		private class UIBuffer
		{
			public int vaoHandle;
			public int vboHandle;
			public int texHandle;
			public bool used;
		}

		private Shader _uiShader;

		private PolygonMode _lastPolygonMode;

		/// <summary>
		/// Buffers that can be repurposed
		/// </summary>
		private Queue<UIBuffer> _reuse;

		/// <summary>
		/// Buffers that were used in the last cycle
		/// </summary>
		private Dictionary<Guid, UIBuffer> _buffers;


		public override void CanvasPreDraw(int width, int height)
		{
			_lastPolygonMode = (PolygonMode)GL.GetInteger(GetPName.PolygonMode);

			_uiShader.Use();
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		public override void CanvasPostDraw()
		{
			foreach(var b in _buffers.Where(x => !x.Value.used).ToArray())
			{
				_reuse.Enqueue(b.Value);
				_buffers.Remove(b.Key);
			}

			foreach(var b in _buffers)
				b.Value.used = false;

			GL.Disable(EnableCap.Blend);
			GL.PolygonMode(MaterialFace.FrontAndBack, _lastPolygonMode);

			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		public override void CanvasDrawUIElement(UIElement element, float width, float height, bool forceUpdateTransforms)
		{
			if(GetUIBuffer(element.ID, out UIBuffer buffer))
			{
				UpdateTransforms(element.GetTransformBuffer(width, height));
				UpdateTexture(element.GetBufferTexture());
			}
			else
			{
				GL.BindVertexArray(buffer.vaoHandle);
				GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.vboHandle);
				GL.BindTexture(TextureTarget.Texture2D, buffer.texHandle);
				if(element.UpdatedTransforms || forceUpdateTransforms)
				{
					UpdateTransforms(element.GetTransformBuffer(width, height));

				}
				if(element.UpdatedTexture)
				{
					UpdateTexture(element.GetBufferTexture());
				}
			}

			buffer.used = true;
			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
		}

		/// <summary>
		/// Returns true if the buffer is not reused
		/// </summary>
		/// <param name="id"></param>
		/// <param name="buffer"></param>
		/// <returns></returns>
		private bool GetUIBuffer(Guid id, out UIBuffer buffer)
		{
			if(!_buffers.TryGetValue(id, out buffer))
			{
				if(_reuse.Count == 0)
				{
					buffer = GenUIBuffer();
					_buffers.Add(id, buffer);
					return true;
				}
				else
					buffer = _reuse.Dequeue();
			}

			return false;
		}

		private UIBuffer GenUIBuffer()
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

			int texHandle = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, texHandle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

			return new UIBuffer()
			{
				vaoHandle = vaoHandle,
				vboHandle = vboHandle,
				texHandle = texHandle
			};
		}

		private unsafe void UpdateTransforms(float[] transformBuffer)
		{
			fixed(float* ptr = transformBuffer)
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



		#endregion
	}

}
