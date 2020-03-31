using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using SonicRetro.SAModel.ModelData.Buffer;
using System.Windows.Input;
using SonicRetro.SAModel.Structs;
using System.IO;

namespace SonicRetro.SAModel.Graphics
{
	/// <summary>
	/// Rendering context base class
	/// </summary>
	public abstract class Context
	{
		/// <summary>
		/// Positiona and resolution of the context
		/// </summary>
		protected Rectangle _screen;

		/// <summary>
		/// Center of the screen, 
		/// </summary>
		protected Point _center;

		/// <summary>
		/// Whether the context was focused in the last update
		/// </summary>
		protected bool _wasFocused;

		// debugging things
		/// <summary>
		/// How wireframes should be displayed
		/// </summary>
		protected WireFrameMode _wireFrameMode;

		/// <summary>
		/// How polygons should be rendered
		/// </summary>
		protected RenderMode _renderMode;

		/// <summary>
		/// Currently active debug overlay
		/// </summary>
		protected DebugMenu _currentDebug;

		/// <summary>
		/// Used for rendering the bounding spheres
		/// </summary>
		protected ModelData.Attach Sphere;

		/// <summary>
		/// Used to render debug information onto
		/// </summary>
		protected Bitmap _debugTexture;

		/// <summary>
		/// Font collection
		/// </summary>
		protected PrivateFontCollection _fonts;

		/// <summary>
		/// Default debug font
		/// </summary>
		protected Font _debugFont;

		/// <summary>
		/// Bold debug font
		/// </summary>
		protected Font _debugFontBold;

		/// <summary>
		/// The resolution of the context
		/// </summary>
		public Size Resolution
		{
			get => _screen.Size;
			set
			{
				_screen.Size = value;
				UpdateScreen(true);
			}
		}

		/// <summary>
		/// The location of the context on the screen
		/// </summary>
		public Point Location
		{
			get => _screen.Location;
			set
			{
				_screen.Location = value;
				UpdateScreen(false);
			}
		}

		/// <summary>
		/// The camera of the scene
		/// </summary>
		public Camera Camera { get; }

		/// <summary>
		/// 3D scene to hold objects and geometry
		/// </summary>
		public Scene Scene { get; }

		/// <summary>
		/// Whether to render Visual models
		/// </summary>
		public bool RenderVisual { get; set; } = true;

		/// <summary>
		/// Whether to render collision models
		/// </summary>
		public bool RenderCollision { get; set; } = false;


		/// <summary>
		/// Creates a new render context
		/// </summary>
		/// <param name="inputUpdater"></param>
		public Context(Rectangle screen, Camera camera, InputUpdater inputUpdater)
		{
			_screen = screen;
			Camera = camera;
			Scene = new Scene(camera);

			_fonts = new PrivateFontCollection();
			_fonts.AddFontFile("debugFont.ttf");
			_debugFont = new Font(_fonts.Families[0], 12);
			_debugFontBold = new Font(_fonts.Families[0], 15, FontStyle.Bold);

			byte[] sphere = File.ReadAllBytes("Sphere.bufmdl");
			uint addr = 0;
			Sphere = new ModelData.Attach(new BufferMesh[] { BufferMesh.Read(sphere, ref addr) })
			{
				Name = "Debug_Sphere"
			};
			BufferMaterial mat = Sphere.MeshData[0].Material;
			mat.MaterialFlags = MaterialFlags.noDiffuse | MaterialFlags.noSpecular;
			mat.UseAlpha = true;
			mat.Culling = true;
			mat.SourceBlendMode = ModelData.BlendMode.SrcAlpha;
			mat.DestinationBlendmode = ModelData.BlendMode.SrcAlphaInverted;
			mat.Ambient = new Structs.Color(128, 64, 64, 32);

			if (File.Exists("Settings.json"))
				Settings.Load("Settings.json");
			else
			{
				Settings.InitDefault();
				Settings.Global.Save("Settings");
			}

			Input.Init(inputUpdater);

			_wireFrameMode = WireFrameMode.None;
			_renderMode = RenderMode.Default;
			_currentDebug = DebugMenu.Disabled;
		}

		public abstract void AsWindow();

		/// <summary>
		/// Called after loading the graphics engine
		/// </summary>
		public abstract void GraphicsInit();

		/// <summary>
		/// Gets called when the
		/// </summary>
		/// <param name="resize">Whether the screen is being resized</param>
		protected virtual void UpdateScreen(bool resize)
		{
			int x = _screen.X + _screen.Width / 2;
			int y = _screen.Y + _screen.Height / 2;
			_center = new Point(x, y);

			if(resize)
			{
				Camera.Aspect = _screen.Width / (float)_screen.Height;
				_debugTexture = new Bitmap(_screen.Width, _screen.Height, PixelFormat.Format32bppArgb);
			}
		}

		public virtual void CircleWireframeMode(bool back)
		{
			switch (_wireFrameMode)
			{
				case WireFrameMode.None:
					_wireFrameMode = back ? WireFrameMode.BoundingSphere : WireFrameMode.Overlay;
					break;
				case WireFrameMode.Overlay:
					_wireFrameMode = back ? WireFrameMode.None : WireFrameMode.ReplaceLine;
					break;
				case WireFrameMode.ReplaceLine:
					_wireFrameMode = back ? WireFrameMode.Overlay : WireFrameMode.ReplacePoint;
					break;
				case WireFrameMode.ReplacePoint:
					_wireFrameMode = back ? WireFrameMode.ReplaceLine : WireFrameMode.BoundingSphere;
					break;
				case WireFrameMode.BoundingSphere:
					_wireFrameMode = back ? WireFrameMode.ReplacePoint : WireFrameMode.None;
					break;
			}
		}

		public virtual void CircleRenderMode(bool back)
		{
			switch (_renderMode)
			{
				case RenderMode.Default:
					_renderMode = back ? RenderMode.CullSide : RenderMode.Smooth;
					break;
				case RenderMode.Smooth:
					_renderMode = back ? RenderMode.Default : RenderMode.Falloff;
					break;
				case RenderMode.Falloff:
					_renderMode = back ? RenderMode.Smooth : RenderMode.Normals;
					break;
				case RenderMode.Normals:
					_renderMode = back ? RenderMode.Falloff : RenderMode.Colors;
					break;
				case RenderMode.Colors:
					_renderMode = back ? RenderMode.Normals : RenderMode.Texcoords;
					break;
				case RenderMode.Texcoords:
					_renderMode = back ? RenderMode.Colors : RenderMode.CullSide;
					break;
				case RenderMode.CullSide:
					_renderMode = back ? RenderMode.Texcoords : RenderMode.Default;
					break;
			}
		}

		public void SetDebugMenu(DebugMenu menu)
		{
			_currentDebug = _currentDebug == menu ? DebugMenu.Disabled : menu;
		}

		public virtual void Update(float delta, bool focused)
		{
			if(focused || _wasFocused)
			{
				Input.Update(_wasFocused);
			}
			if(focused)
			{
				Settings s = Settings.Global;
				if(!Camera.Orbiting) // if in fps mode
				{
					if (!_wasFocused || Input.IsKeyDown(Key.Escape))
						Camera.Orbiting = true;
					else Input.PlaceCursor(_center);
				}
				else if (Input.KeyPressed(s.navMode))
				{
					Camera.Orbiting = false;
					Input.PlaceCursor(_center);
				}
				else
				{
					if(Input.KeyPressed(s.focusObj))
					{
						// todo
					}
				}

				Camera.Move(delta);

				bool backWard = Input.IsKeyDown(s.circleBackward);
				if (Input.KeyPressed(s.circleLighting)) CircleRenderMode(backWard);
				if (Input.KeyPressed(s.circleWireframe)) CircleWireframeMode(backWard);
				if(Input.KeyPressed(s.swapGeometry))
				{
					RenderCollision = RenderVisual;
					RenderVisual = !RenderVisual;
				}

				if (Input.KeyPressed(s.DebugHelp)) 
					SetDebugMenu(DebugMenu.Help);
				else if (Input.KeyPressed(s.DebugCamera)) SetDebugMenu(DebugMenu.Camera);
				else if (Input.KeyPressed(s.DebugRender)) SetDebugMenu(DebugMenu.RenderInfo);
			}
			_wasFocused = focused;

			Scene.Update(delta);
		}

		public virtual void Render()
		{
			RenderMaterial.ViewPos = Camera.Realposition;
			RenderMaterial.LightDir = new Vector3(0, 1, 0);
			RenderMaterial.ViewDir = Camera.Orthographic ? Camera.ViewDir : default;
			RenderMaterial.RenderMode = _renderMode;
		}

		/// <summary>
		/// Renders the debug texture
		/// </summary>
		protected virtual void RenderDebug(uint modelsDrawn)
		{
			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_debugTexture))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

				int yOffset = 45;
				int lineHeight = _debugFont.Height;
				Brush br = Brushes.White;
				Brush bg = new SolidBrush(System.Drawing.Color.FromArgb(0x80, 0, 0, 0));

				void text(string str, int xOffset)
				{
					g.DrawString(str, _debugFont, br, xOffset, yOffset);
					yOffset += lineHeight;
				}
				void textBold(string str, int xOffset)
				{
					g.DrawString(str, _debugFontBold, br, xOffset, yOffset);
					yOffset += lineHeight;
				}

				g.Clear(System.Drawing.Color.Transparent);
				switch (_currentDebug)
				{
					case DebugMenu.Help:
						g.FillRectangle(bg, 0, 0, 200, 150);
						textBold("== Debug == ", 30);
						text("Debug      - F1", 10);
						text("Camera     - F2", 10);
						text("Renderinfo - F3", 10);
						break;
					case DebugMenu.Camera:
						g.FillRectangle(bg, 0, 0, 350, 225);
						textBold("== Camera == ", 90);
						text($"Location: {Camera.Position.Rounded(2)}", 10);
						text($"Rotation: {Camera.Rotation.Rounded(2)}", 10);
						text($"Distance: {Camera.Distance}", 10);
						text($"View type: " + (Camera.Orthographic ? "Orthographic" : $"Perspective - FoV: {Camera.FieldOfView}"), 10);
						text($"View Distance: {Camera.ViewDistance}", 10);
						text($"Nav mode: " + (Camera.Orbiting ? "Orbiting" : "First Person"), 10);
						text($"Move speed: {Camera.MovementSpeed}", 10);
						text($"Mouse speed: {Camera.MouseSensitivity}", 10);
						break;
					case DebugMenu.RenderInfo:
						g.FillRectangle(bg, 0, 0, 350, 230);
						textBold("== Renderinfo == ", 60);
						text($"View Pos.: {RenderMaterial.ViewPos.Rounded(2)}", 10);
						text($"Lighting Dir.: {RenderMaterial.LightDir.Rounded(2)}", 10);
						text($"Models Drawn: {modelsDrawn}", 10);
						text($"Render Mode: {_renderMode}", 10);
						text($"Wireframe Mode: {_wireFrameMode}", 10);
						text("Display:", 10);
						text($"Visual: {RenderVisual}", 20);
						text($"Collision: {RenderCollision}", 20);
						break;
				}

				g.Flush();
			}
		}
	}
}
