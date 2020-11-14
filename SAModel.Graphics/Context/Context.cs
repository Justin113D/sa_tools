using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Windows.Input;
using System.IO;
using SonicRetro.SAModel.ModelData.Buffer;
using SonicRetro.SAModel.Structs;
using SonicRetro.SAModel.ObjData;
using Color = SonicRetro.SAModel.Structs.Color;
using System;
using System.Windows.Interop;

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
		/// Whether to draw bounds
		/// </summary>
		protected bool _drawBounds;

		public Color BackgroundColor { get; protected set; }

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
		/// Whether to render collision models
		/// </summary>
		public bool _renderCollision = false;

		/// <summary>
		/// Active NJ Object
		/// </summary>
		protected NJObject ActiveObj { get; set; }

		/// <summary>
		/// Active geometry object
		/// </summary>
		protected LandEntry ActiveLE { get; set; }

		/// <summary>
		/// Whether the context is focused
		/// </summary>
		public bool Focused { get; set; }

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
			BackgroundColor = new Color(0x20, 0x20, 0x20);

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
			mat.Ambient = new Color(128, 128, 128, 64);

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

		/// <summary>
		/// Starts the context as an independent window
		/// </summary>
		public abstract void AsWindow();

		public abstract System.Windows.FrameworkElement AsControl(HwndSource windowSource);

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
					_wireFrameMode = back ? WireFrameMode.ReplacePoint : WireFrameMode.Overlay;
					break;
				case WireFrameMode.Overlay:
					_wireFrameMode = back ? WireFrameMode.None : WireFrameMode.ReplaceLine;
					break;
				case WireFrameMode.ReplaceLine:
					_wireFrameMode = back ? WireFrameMode.Overlay : WireFrameMode.ReplacePoint;
					break;
				case WireFrameMode.ReplacePoint:
					_wireFrameMode = back ? WireFrameMode.ReplaceLine : WireFrameMode.None;
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
					_renderMode = back ? RenderMode.Falloff : RenderMode.ColorsWeights;
					break;
				case RenderMode.ColorsWeights:
					_renderMode = back ? RenderMode.Normals : RenderMode.Texcoords;
					break;
				case RenderMode.Texcoords:
					_renderMode = back ? RenderMode.ColorsWeights : RenderMode.CullSide;
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

		/// <summary>
		/// Sets the active (seleted) model
		/// </summary>
		public void SelectActive(NJObject njobj)
		{
			if (njobj == null) return;
			ActiveObj = njobj;
			ActiveLE = null;
		}

		/// <summary>
		/// Sets the active (seleted) model
		/// </summary>
		public void SelectActive(LandEntry le)
		{
			if (le == null) return;
			ActiveObj = null;
			ActiveLE = le;
		}

		public virtual void Update(double delta)
		{
			if(Focused || _wasFocused)
			{
				Input.Update(_wasFocused);
			}
			if(Focused)
			{
				Settings s = Settings.Global;
				bool backWard = Input.IsKeyDown(s.circleBackward);

				if (!Camera.Orbiting) // if in fps mode
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

						// temporary
						if(Scene.geometry.Count == 0)
						{
							if (ActiveObj == null) ActiveObj = Scene.objects[0].obj;
							else
							{
								NJObject[] objects = Scene.objects[0].obj.GetObjects();
								for(int i = 0; i < objects.Length; i++)
								{
									if(objects[i] == ActiveObj)
									{
										i += backWard ? -1 : 1;
										if (i == -1) i = objects.Length - 1;
										else if (i == objects.Length) i = 0;
										ActiveObj = objects[i];
										break;
									}
								}
							}
						}
						else
						{
							if (ActiveLE == null) ActiveLE = Scene.geometry[0];
							else
							{
								for (int i = 0; i < Scene.geometry.Count; i++)
								{
									if (Scene.geometry[i] == ActiveLE)
									{
										i += backWard ? -1 : 1;
										if (i == -1) i = Scene.geometry.Count - 1;
										else if (i == Scene.geometry.Count) i = 0;
										ActiveLE = Scene.geometry[i];
										Camera.Position = ActiveLE.ModelBounds.Position;
										break;
									}
								}
							}
						}
					}
				}

				Camera.Move((float)delta);

				if (Input.KeyPressed(s.circleLighting)) CircleRenderMode(backWard);
				if (Input.KeyPressed(s.circleWireframe)) CircleWireframeMode(backWard);
				if (Input.KeyPressed(s.swapGeometry)) _renderCollision = !_renderCollision;
				if (Input.KeyPressed(s.displayBounds)) _drawBounds = !_drawBounds;

				if (Input.KeyPressed(s.DebugHelp)) SetDebugMenu(DebugMenu.Help);
				else if (Input.KeyPressed(s.DebugCamera)) SetDebugMenu(DebugMenu.Camera);
				else if (Input.KeyPressed(s.DebugRender)) SetDebugMenu(DebugMenu.RenderInfo);
			}
			_wasFocused = Focused;

			Scene.Update(delta);
		}

		public virtual void Render()
		{
			RenderMaterial.ViewPos = Camera.Realposition;
			RenderMaterial.LightDir = new Vector3(0, 1, 0);
			RenderMaterial.ViewDir = Camera.Orthographic ? Camera.ViewDir : default;
			RenderMaterial.RenderMode = _renderMode;
		}

		//byte value = 0;

		/// <summary>
		/// Renders the debug texture
		/// </summary>
		protected virtual void RenderDebug(uint meshesDrawn)
		{
			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_debugTexture))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

				int yOffset = 5;
				int lineHeight = _debugFont.Height;
				Brush br = Brushes.White;
				Brush bg = new SolidBrush(System.Drawing.Color.FromArgb(0x60, 0x60, 0x60, 0x60));
				//if (value == 0xFF) value = 0;
				//else value++;
				

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

				g.Clear(System.Drawing.Color.FromArgb(0));
				switch (_currentDebug)
				{
					case DebugMenu.Help:
						g.FillRectangle(bg, 0, 0, 200, 105);
						textBold("== Debug == ", 30);
						text("Debug      - F1", 10);
						text("Camera     - F2", 10);
						text("Renderinfo - F3", 10);
						break;
					case DebugMenu.Camera:
						g.FillRectangle(bg, 0, 0, 350, 180);
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
						g.FillRectangle(bg, 0, 0, 350, 180);
						textBold("== Renderinfo == ", 60);
						text($"View Pos.: {RenderMaterial.ViewPos.Rounded(2)}", 10);
						text($"Lighting Dir.: {RenderMaterial.LightDir.Rounded(2)}", 10);
						text($"Meshes Drawn: {meshesDrawn}", 10);
						text($"Render Mode: {_renderMode}", 10);
						text($"Wireframe Mode: {_wireFrameMode}", 10);
						text($"Display: {(_renderCollision ? "Collision" : "Visual")}", 10);
						text($"Display Bounds: {_drawBounds}", 10);
						if (ActiveObj != null) text($"Active object: {ActiveObj.Name}", 10);
						else if (ActiveLE != null)
						{
							text($"Active object: LandEntry {Scene.geometry.IndexOf(ActiveLE)}", 10);
						}
						else text($"Active object: NULL", 10);
						break;
				}

				g.Flush();
			}
		}
	}
}
