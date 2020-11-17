using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Platform;
using OpenTK.Wpf;
using SonicRetro.SAModel.Graphics.OpenGL.Properties;
using SonicRetro.SAModel.Graphics.OpenGL.Rendering;
using SonicRetro.SAModel.Graphics.UI;
using SonicRetro.SAModel.ObjData;
using SonicRetro.SAModel.Structs;

namespace SonicRetro.SAModel.Graphics.OpenGL
{
	public class GLDebugContext : DebugContext
	{
		/// <summary>
		/// The shader to use
		/// </summary>
		public Shader Shader { get; private set; }

		public GLCamera GLCam => (GLCamera)Camera;

		public GLCanvas GLCanvas => (GLCanvas)Canvas;

		public GLDebugContext(Rectangle screen) : this(screen, new GLCamera(screen.Width / (float)screen.Height), new GLCanvas(), new GLInputUpdater()) { }

		public GLDebugContext(Rectangle screen, GLCamera camera, GLCanvas canvas, GLInputUpdater inputUpdater) : base(screen, camera, canvas, inputUpdater) { }

		public override FrameworkElement AsControl(HwndSource windowSource)
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

		public override void AsWindow()
		{
			var wnd = new GLWindow(this, _screen.Width, _screen.Height);
			Location = wnd.Location;
			wnd.Run(60, 60);
		}

		public override void GraphicsInit()
		{

			GL.Viewport(default, _screen.Size);
			GL.ClearColor(BackgroundColor.SystemCol);
			GL.Enable(EnableCap.DepthTest);
			GL.Uniform1(13, 0f); // setting normal offset for wireframe

			// loading the shaders
			string vertexShader = Encoding.ASCII.GetString(Resources.VertexShader).Trim('?');
			string fragShader = Encoding.ASCII.GetString(Resources.FragShader).Trim('?');
			Shader = new Shader(vertexShader, fragShader);

			GLMaterial.Init();
			Shader.BindUniformBlock("Material", 0, GLMaterial.Handle);

			_sphere.Buffer(null, false);

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

		public override void CircleWireframeMode(bool back)
		{
			base.CircleWireframeMode(back);
			switch(_wireFrameMode)
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

		protected override void ContextRender()
		{
			if(Shader == null)
				return;
			GLMaterial.ViewPos = Camera.Realposition;
			GLMaterial.ViewDir = Camera.Orthographic ? Camera.ViewDir : default;
			GLMaterial.RenderMode = _renderMode;

			Extensions.ClearWeights();

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			List<GLRenderMesh> renderMeshes = new List<GLRenderMesh>();
			List<LandEntry> entries = new List<LandEntry>();

			if(!_renderCollision)
			{
				foreach(LandEntry le in Scene.VisualGeometry)
					RenderMethods.PrepareLandentry(renderMeshes, entries, GLCam, ActiveLE, le);
				foreach(GameTask tsk in Scene.objects)
				{
					tsk.Display();
					RenderMethods.PrepareModel(renderMeshes, GLCam, ActiveObj, tsk.obj, null, tsk.obj.HasWeight);
				}
			}
			else
				foreach(LandEntry le in Scene.CollisionGeometry)
					RenderMethods.PrepareLandentry(renderMeshes, entries, GLCam, ActiveLE, le);

			Shader.Use();
			if(_renderCollision)
				GLMaterial.RenderMode = RenderMode.FullBright;

			// first the opaque meshes
			RenderMethods.RenderModels(renderMeshes, false);
			if(_wireFrameMode == WireFrameMode.Overlay)
				RenderMethods.RenderModelsWireframe(renderMeshes, false);

			// then transparent meshes
			GL.Enable(EnableCap.Blend);

			// then the transparent meshes
			RenderMethods.RenderModels(renderMeshes, true);

			if(_wireFrameMode == WireFrameMode.Overlay)
				RenderMethods.RenderModelsWireframe(renderMeshes, true);

			if(_boundsMode != BoundsMode.None && ActiveLE != null)
			{
				GL.Disable(EnableCap.DepthTest);
				GLMaterial.RenderMode = RenderMode.Falloff;
				Matrix4 normal = Matrix4.Identity;
				GL.UniformMatrix4(11, false, ref normal);

				List<LandEntry> boundObjs;

				if(_boundsMode == BoundsMode.All)
				{
					boundObjs = Scene.geometry;
				}
				else
				{
					boundObjs = new List<LandEntry>
					{
						ActiveLE
					};
				}

				foreach(LandEntry le in boundObjs)
				{
					Bounds b = le.ModelBounds;
					Matrix4 world = Matrix4.CreateScale(b.Radius) * Matrix4.CreateTranslation(b.Position.ToGL());
					GL.UniformMatrix4(10, false, ref world);
					world = world * GLCam.ViewMatrix * GLCam.Projectionmatrix;
					GL.UniformMatrix4(12, false, ref world);
					_sphere.Render(null, true, false);
				}

				GL.Enable(EnableCap.DepthTest);
			}
			GL.Disable(EnableCap.Blend);

			DrawDebug((uint)renderMeshes.Count);
		}
	}
}
