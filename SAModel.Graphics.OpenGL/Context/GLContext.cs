using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using SonicRetro.SAModel.Graphics.OpenGL.Properties;
using SonicRetro.SAModel.Graphics.OpenGL.Rendering;
using SonicRetro.SAModel.ObjData;
using SonicRetro.SAModel.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Wpf;
using OpenTK.Platform;
using System.Windows.Interop;

namespace SonicRetro.SAModel.Graphics.OpenGL
{
	struct RenderMesh
	{
		public ModelData.Attach attach;
		public Matrix4? realWorldMtx;
		public Matrix4 worldMtx;
		public Matrix4 normalMtx;
		public Matrix4 MVP;
		public bool active;

		public RenderMesh(ModelData.Attach attach, Matrix4? realWorldMtx, Matrix4 worldMtx, Matrix4 normalMtx, Matrix4 mVP, bool active)
		{
			this.attach = attach;
			this.realWorldMtx = realWorldMtx;
			this.worldMtx = worldMtx;
			this.normalMtx = normalMtx;
			MVP = mVP;
			this.active = active;
		}
	}

	public class GLContext : Context
	{
		private int _nearPlaneHandle;

		private int _debugTextureHandle;
		private Shader _debugShader;

		/// <summary>
		/// The shader to use
		/// </summary>
		public Shader Shader { get; private set; }

		public GLCamera GLCam => (GLCamera)Camera;

		public GLContext(Rectangle screen) : base(screen, new GLCamera(screen.Width / (float)screen.Height), new GLInputUpdater())
		{
		}

		// starts this context as a standalone window
		public override void AsWindow()
		{
			var wnd = new GLContextWindow(this, _screen.Width, _screen.Height);
			Location = wnd.Location;
			wnd.Run(60, 60);
		}

		public override System.Windows.FrameworkElement AsControl(HwndSource windowSource)
		{
			GLWpfControl control = new GLWpfControl();
			control.Ready += GraphicsInit;
			
			control.Render += (time) =>
			{
				Update(time.TotalSeconds);

				Render();
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

		unsafe private void BufferNearPlane()
		{
			// buffering the nearplane 
			_nearPlaneHandle = GL.GenVertexArray();

			GL.BindVertexArray(_nearPlaneHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, GL.GenBuffer());

			sbyte[] posUV = new sbyte[] {   -1, -1, 0, 1,
											 1, -1, 1, 1,
											-1,  1, 0, 0,
											 1,  1, 1, 0 };
			fixed (sbyte* ptr = posUV)
			{
				GL.BufferData(BufferTarget.ArrayBuffer, posUV.Length, (IntPtr)ptr, BufferUsageHint.StaticDraw);
			}

			// assigning attribute data
			// position
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Byte, false, 4, 0);

			// uv
			GL.EnableVertexAttribArray(3);
			GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Byte, false, 4, 2);

			//unbind buffers
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		public override void GraphicsInit()
		{
			GL.Viewport(default, _screen.Size);
			GL.ClearColor(BackgroundColor.SystemCol);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.FramebufferSrgb);
			GL.Uniform1(13, 0f); // setting normal offset for wireframe

			GLMaterial.Init();
			BufferNearPlane();

			// loading the shaders
			string vertexShader = System.Text.Encoding.ASCII.GetString(Resources.VertexShader).Trim('?');
			string fragShader = System.Text.Encoding.ASCII.GetString(Resources.FragShader).Trim('?');
			Shader = new Shader(vertexShader, fragShader);
			Shader.BindUniformBlock("Material", 0, GLMaterial.Handle);

			// loading the shaders
			vertexShader = System.Text.Encoding.ASCII.GetString(Resources.DebugVert).Trim('?');
			fragShader = System.Text.Encoding.ASCII.GetString(Resources.DebugFrag).Trim('?');
			_debugShader = new Shader(vertexShader, fragShader);

			_debugTextureHandle = GL.GenTexture();

			// buffering the (unweightened) meshes

			Sphere.Buffer(null, false);

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		protected override void UpdateScreen(bool resize)
		{
			base.UpdateScreen(resize);
			if (resize)
			{
				GL.Viewport(default, Resolution);
			}
		}

		public override void CircleWireframeMode(bool back)
		{
			base.CircleWireframeMode(back);
			switch (_wireFrameMode)
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

		public override void Render()
		{
			if (Shader == null) return;
			base.Render();
			Extensions.ClearWeights();

			// clear 
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			List<RenderMesh> renderMeshes = new List<RenderMesh>();
			List<LandEntry> entries = new List<LandEntry>();

			void PrepareModel(NJObject obj, Matrix4? parentWorld, bool weighted)
			{
				Matrix4 world = obj.LocalMatrix();
				if (parentWorld.HasValue)
					world *= parentWorld.Value;

				if (obj.Attach != null)
				{
					// if a model is weighted, then the buffered vertex positions/normals will have to be set to world space, which means that world and normal matrix should be identities
					if (weighted)
					{
						renderMeshes.Add(new RenderMesh(obj.Attach, world, Matrix4.Identity, Matrix4.Identity, GLCam.ViewMatrix * GLCam.Projectionmatrix, obj == ActiveObj));
					}
					else
					{
						Matrix4 normalMtx = world.Inverted();
						normalMtx.Transpose();
						renderMeshes.Add(new RenderMesh(obj.Attach, null, world, normalMtx, world * GLCam.ViewMatrix * GLCam.Projectionmatrix, obj == ActiveObj));
					}
				}

				for (int i = 0; i < obj.ChildCount; i++)
					PrepareModel(obj[i], world, weighted);
			}

			void PrepareLandentry(LandEntry le)
			{
				if (!GLCam.Renderable(le) || entries.Contains(le))
					return;
				entries.Add(le);
				Matrix4 world = le.LocalMatrix();
				Matrix4 normalMtx = world.Inverted();
				normalMtx.Transpose();
				renderMeshes.Add(new RenderMesh(le.Attach, null, world, normalMtx, world * GLCam.ViewMatrix * GLCam.Projectionmatrix, le == ActiveLE));
			}

			if (!_renderCollision)
			{
				foreach (LandEntry le in Scene.VisualGeometry)
					PrepareLandentry(le);
				foreach (GameTask tsk in Scene.objects)
				{
					tsk.Display();
					PrepareModel(tsk.obj, null, tsk.obj.HasWeight);
				}
			}
			else foreach (LandEntry le in Scene.CollisionGeometry)
					PrepareLandentry(le);

			void RenderModels(bool transparent)
			{
				for (int i = 0; i < renderMeshes.Count; i++)
				{
					RenderMesh m = renderMeshes[i];
					GL.UniformMatrix4(10, false, ref m.worldMtx);
					GL.UniformMatrix4(11, false, ref m.normalMtx);
					GL.UniformMatrix4(12, false, ref m.MVP);
					m.attach.Render(m.realWorldMtx, transparent, m.active);
				}
			}

			void RenderModelsWireframe(bool transparent)
			{
				GL.Uniform1(13, 0.001f); // setting normal offset for wireframe
				RenderMode old = RenderMaterial.RenderMode;
				RenderMaterial.RenderMode = RenderMode.FullDark; // drawing the lines black
				GLMaterial.Reset();
				GLMaterial.Buffer();
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

				for (int i = 0; i < renderMeshes.Count; i++)
				{
					RenderMesh m = renderMeshes[i];
					GL.UniformMatrix4(10, false, ref m.worldMtx);
					GL.UniformMatrix4(11, false, ref m.normalMtx);
					GL.UniformMatrix4(12, false, ref m.MVP);
					m.attach.RenderWireframe(transparent);
				}

				// reset
				GL.Uniform1(13, 0f);
				RenderMaterial.RenderMode = old;
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}

			Shader.Use();
			if (_renderCollision) RenderMaterial.RenderMode = RenderMode.FullBright;

			// first the opaque meshes
			RenderModels(false);
			if (_wireFrameMode == WireFrameMode.Overlay)
				RenderModelsWireframe(false);

			GL.Enable(EnableCap.Blend);


			// then the transparent meshes
			RenderModels(true);
			if (_wireFrameMode == WireFrameMode.Overlay)
				RenderModelsWireframe(true);

			if (_drawBounds && ActiveLE != null)
			{
				GL.Disable(EnableCap.DepthTest);
				RenderMaterial.RenderMode = RenderMode.Falloff;
				Matrix4 normal = Matrix4.Identity;
				GL.UniformMatrix4(11, false, ref normal);

				/*foreach(LandEntry le in Scene.geometry)
				{
					Bounds b = le.ModelBounds;
					Matrix4 world = Matrix4.CreateScale(b.Radius) * Matrix4.CreateTranslation(b.Position.ToGL());
					GL.UniformMatrix4(10, false, ref world);
					world = world * GLCam.ViewMatrix * GLCam.Projectionmatrix;
					GL.UniformMatrix4(12, false, ref world);
					Sphere.Render(null, true, false);
				}*/

				Bounds b = ActiveLE.ModelBounds;
				Matrix4 world = Matrix4.CreateScale(b.Radius) * Matrix4.CreateTranslation(b.Position.ToGL());
				GL.UniformMatrix4(10, false, ref world);
				world = world * GLCam.ViewMatrix * GLCam.Projectionmatrix;
				GL.UniformMatrix4(12, false, ref world);
				Sphere.Render(null, true, false);
				
				GL.Enable(EnableCap.DepthTest);
			}
			GL.Disable(EnableCap.Blend);

			RenderDebug((uint)renderMeshes.Count);
		}

		protected override void RenderDebug(uint modelsDrawn)
		{
			if (_currentDebug == DebugMenu.Disabled) return;
			base.RenderDebug(modelsDrawn);

			// buffering
			Rectangle canvas = new Rectangle(new Point(), Resolution);
			BitmapData data = _debugTexture.LockBits(canvas, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.BindTexture(TextureTarget.Texture2D, _debugTextureHandle);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			_debugTexture.UnlockBits(data);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

			// drawing the texture onto the screen
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			GL.Enable(EnableCap.Blend);
			GL.BindVertexArray(_nearPlaneHandle);
			_debugShader.Use();
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			GL.Disable(EnableCap.Blend);

			switch (_wireFrameMode)
			{
				case WireFrameMode.ReplaceLine:
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
					break;
				case WireFrameMode.ReplacePoint:
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
					break;
			}
		}

	}

	public class GLContextWindow : GameWindow
	{
		private readonly GLContext _context;

		private bool _showedCursor;

		public GLContextWindow(GLContext context, int width, int height) :
			base(
				width,
				height,
				new GraphicsMode(ColorFormat.Empty, 24, 0, 4),
				"SA3D",  // initial title
				GameWindowFlags.Default,
				DisplayDevice.Default,
				4, // OpenGL major version
				0, // OpenGL minor version
				GraphicsContextFlags.ForwardCompatible)
		{
			_context = context;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			_context.GraphicsInit();
			_context.Location = Location;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			_context.Resolution = Size;
		}

		protected override void OnMove(EventArgs e)
		{
			base.OnMove(e);
			_context.Location = Location;
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
			_context.Focused = Focused;
			_context.Update((float)e.Time);
			if (_showedCursor == _context.Camera.Orbiting)
			{
				CursorVisible = _context.Camera.Orbiting;
				_showedCursor = !_context.Camera.Orbiting;
			}
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			_context.Render();
			Context.SwapBuffers();
		}
	}
}
