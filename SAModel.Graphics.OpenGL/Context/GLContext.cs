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

	public class GLContext : Context
	{
		/// <summary>
		/// The shader to use
		/// </summary>
		public Shader Shader { get; private set; }

		public GLCamera GLCam => (GLCamera)Camera;

		public GLCanvas GLCanvas => (GLCanvas)Canvas;

		public GLContext(Rectangle screen) : base(screen, new GLCamera(screen.Width / (float)screen.Height), new GLCanvas(), new GLInputUpdater())
		{
		}

		// starts this context as a standalone window
		public override void AsWindow()
		{
			var wnd = new GLWindow(this, _screen.Width, _screen.Height);
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

		public override void GraphicsInit()
		{
			GL.Viewport(default, _screen.Size);
			GL.ClearColor(BackgroundColor.SystemCol);
			GL.Enable(EnableCap.DepthTest);
			GL.Uniform1(13, 0f); // setting normal offset for wireframe

			GLMaterial.Init();

			// loading the shader
			string vertexShader = System.Text.Encoding.ASCII.GetString(Resources.VertexShader).Trim('?');
			string fragShader = System.Text.Encoding.ASCII.GetString(Resources.FragShader).Trim('?');
			Shader = new Shader(vertexShader, fragShader);
			Shader.BindUniformBlock("Material", 0, GLMaterial.Handle);

			GLCanvas.GraphicsInit();

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		protected override void UpdateScreen(bool resize)
		{
			base.UpdateScreen(resize);
			if(resize)
			{
				GL.Viewport(default, Resolution);
			}
		}

		protected override void ContextUpdate(double delta) { }

		protected override void ContextRender()
		{
			if(Shader == null)
				return;
			GLMaterial.ViewPos = Camera.Realposition;
			GLMaterial.ViewDir = Camera.Orthographic ? Camera.ViewDir : default;

			Extensions.ClearWeights();

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			List<GLRenderMesh> renderMeshes = new List<GLRenderMesh>();
			List<LandEntry> entries = new List<LandEntry>();

			foreach(LandEntry le in Scene.VisualGeometry)
				RenderMethods.PrepareLandentry(renderMeshes, entries, GLCam, null, le);
			foreach(GameTask tsk in Scene.objects)
			{
				tsk.Display();
				RenderMethods.PrepareModel(renderMeshes, GLCam, null, tsk.obj, null, tsk.obj.HasWeight);
			}

			Shader.Use();

			// first the opaque meshes
			RenderMethods.RenderModels(renderMeshes, false);

			// then transparent meshes
			GL.Enable(EnableCap.Blend);
			RenderMethods.RenderModels(renderMeshes, true);
			GL.Disable(EnableCap.Blend);
		}
	}
}
