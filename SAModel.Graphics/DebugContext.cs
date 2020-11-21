using SonicRetro.SAModel.ModelData.Buffer;
using SonicRetro.SAModel.ObjData;
using Color = SonicRetro.SAModel.Structs.Color;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Input;
using System.Drawing.Imaging;
using SonicRetro.SAModel.Graphics.UI;

namespace SonicRetro.SAModel.Graphics
{
	public abstract class DebugContext : Context
	{
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
		protected BoundsMode _boundsMode;

		/// <summary>
		/// Used for rendering the bounding spheres
		/// </summary>
		protected ModelData.Attach _sphere;

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

		private readonly UIImage _debugPanel;

		protected DebugContext(Rectangle screen, Camera camera, Canvas canvas, InputUpdater inputUpdater) : base(screen, camera, canvas, inputUpdater)
		{
			_fonts = new PrivateFontCollection();
			_fonts.AddFontFile("debugFont.ttf");
			_debugFont = new Font(_fonts.Families[0], 12);
			_debugFontBold = new Font(_fonts.Families[0], 15, FontStyle.Bold);

			byte[] sphere = File.ReadAllBytes("Sphere.bufmdl");
			uint addr = 0;
			_sphere = new ModelData.Attach(new BufferMesh[] { BufferMesh.Read(sphere, ref addr) })
			{
				Name = "Debug_Sphere"
			};
			BufferMaterial mat = _sphere.MeshData[0].Material;
			mat.MaterialFlags = MaterialFlags.noDiffuse | MaterialFlags.noSpecular;
			mat.UseAlpha = true;
			mat.Culling = true;
			mat.SourceBlendMode = ModelData.BlendMode.SrcAlpha;
			mat.DestinationBlendmode = ModelData.BlendMode.SrcAlphaInverted;
			mat.Ambient = new Color(128, 128, 128, 64);

			if(File.Exists("Settings.json"))
				DebugSettings.Load("Settings.json");
			else
			{
				DebugSettings.InitDefault();
				DebugSettings.Global.Save("Settings");
			}

			_wireFrameMode = WireFrameMode.None;
			_renderMode = RenderMode.Default;
			_currentDebug = DebugMenu.Disabled;

			_debugPanel = new UIImage(default, new Structs.Vector2(0, 1), new Structs.Vector2(0, 1), 0, null);
		}

		/// <summary>
		/// Circling base function
		/// </summary>
		/// <param name="value"></param>
		/// <param name="count"></param>
		/// <param name="back"></param>
		/// <returns></returns>
		private int Circle(int value, int count, bool back)
		{
			count--;
			_ = back ? value-- : value++;

			if(value < 0)
				value = count;
			else if(value > count)
				value = 0;

			return value;
		}

		public virtual void CircleWireframeMode(bool back)
		{
			_wireFrameMode = (WireFrameMode)Circle((int)_wireFrameMode, Enum.GetValues(typeof(WireFrameMode)).Length, back);
		}

		public virtual void CircleRenderMode(bool back)
		{
			_renderMode = (RenderMode)Circle((int)_renderMode, Enum.GetValues(typeof(RenderMode)).Length, back);
			if(_renderMode == RenderMode.FullBright)
				_renderMode = RenderMode.Normals;
			else if(_renderMode == RenderMode.FullDark)
				_renderMode = RenderMode.Falloff;
		}

		public virtual void CircleBoundsMode(bool back)
		{
			_boundsMode = (BoundsMode)Circle((int)_boundsMode, Enum.GetValues(typeof(BoundsMode)).Length, back);
		}

		public void SetDebugMenu(DebugMenu menu)
		{
			_currentDebug = _currentDebug == menu ? DebugMenu.Disabled : menu;

			if(_currentDebug == DebugMenu.Disabled)
				return;

			int width = 0;
			int height = 0;
			switch(_currentDebug)
			{
				case DebugMenu.Help:
					width = 200;
					height = 105;
					break;
				case DebugMenu.Camera:
				case DebugMenu.RenderInfo:
					width = 350;
					height = 180;
					break;
			}
			_debugTexture = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			_debugPanel.Texture = _debugTexture;
		}

		/// <summary>
		/// Sets the active (seleted) model
		/// </summary>
		public void SelectActive(NJObject njobj)
		{
			if(njobj == null)
				return;
			ActiveObj = njobj;
			ActiveLE = null;
		}

		/// <summary>
		/// Sets the active (seleted) model
		/// </summary>
		public void SelectActive(LandEntry le)
		{
			if(le == null)
				return;
			ActiveObj = null;
			ActiveLE = le;
		}

		protected override void ContextUpdate(double delta)
		{
			if(Focused == this)
			{
				DebugSettings s = DebugSettings.Global;
				bool backWard = Input.IsKeyDown(s.circleBackward);

				if(!Camera.Orbiting) // if in fps mode
				{
					if(!_wasFocused || Input.IsKeyDown(Key.Escape))
						Camera.Orbiting = true;
					else
						Input.PlaceCursor(Context.Focused.Center);
				}
				else if(Input.KeyPressed(s.navMode))
				{
					Camera.Orbiting = false;
					Input.PlaceCursor(Context.Focused.Center);
				}
				else
				{
					if(Input.KeyPressed(s.focusObj))
					{
						// todo

						// temporary
						if(Scene.geometry.Count == 0)
						{
							if(Scene.objects.Count > 0)
							{
								if(ActiveObj == null)
									ActiveObj = Scene.objects[0].obj;
								else
								{
									NJObject[] objects = Scene.objects[0].obj.GetObjects();
									for(int i = 0; i < objects.Length; i++)
									{
										if(objects[i] == ActiveObj)
										{
											i += backWard ? -1 : 1;
											if(i == -1)
												i = objects.Length - 1;
											else if(i == objects.Length)
												i = 0;
											ActiveObj = objects[i];
											break;
										}
									}
								}
							}
						}
						else
						{
							if(ActiveLE == null)
								ActiveLE = Scene.geometry[0];
							else
							{
								for(int i = 0; i < Scene.geometry.Count; i++)
								{
									if(Scene.geometry[i] == ActiveLE)
									{
										i += backWard ? -1 : 1;
										if(i == -1)
											i = Scene.geometry.Count - 1;
										else if(i == Scene.geometry.Count)
											i = 0;
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

				if(Input.KeyPressed(s.circleLighting))
					CircleRenderMode(backWard);
				if(Input.KeyPressed(s.circleWireframe))
					CircleWireframeMode(backWard);
				if(Input.KeyPressed(s.displayBounds))
					CircleBoundsMode(backWard);
				if(Input.KeyPressed(s.swapGeometry))
					_renderCollision = !_renderCollision;

				if(Input.KeyPressed(s.DebugHelp))
					SetDebugMenu(DebugMenu.Help);
				else if(Input.KeyPressed(s.DebugCamera))
					SetDebugMenu(DebugMenu.Camera);
				else if(Input.KeyPressed(s.DebugRender))
					SetDebugMenu(DebugMenu.RenderInfo);
			}
		}

		/// <summary>
		/// Renders the debug texture
		/// </summary>
		protected virtual void DrawDebug(uint meshesDrawn)
		{
			if(_currentDebug == DebugMenu.Disabled)
				return;

			using(System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_debugTexture))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

				int yOffset = 5;
				int lineHeight = _debugFont.Height;
				Brush br = Brushes.White;


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

				g.Clear(System.Drawing.Color.FromArgb(0x60, 0, 0, 0));
				switch(_currentDebug)
				{
					case DebugMenu.Help:
						textBold("== Debug == ", 30);
						text("Debug      - F1", 10);
						text("Camera     - F2", 10);
						text("Renderinfo - F3", 10);
						break;
					case DebugMenu.Camera:
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
						textBold("== Renderinfo == ", 60);
						text($"View Pos.: {Camera.Realposition.Rounded(2)}", 10);
						//text($"Lighting Dir.: LIGHTDATA TODO", 10);//{RenderMaterial.LightDir.Rounded(2)}", 10);
						text($"Meshes Drawn: {meshesDrawn}", 10);
						text($"Render Mode: {_renderMode}", 10);
						text($"Wireframe Mode: {_wireFrameMode}", 10);
						text($"Display: {(_renderCollision ? "Collision" : "Visual")}", 10);
						text($"Display Bounds: {_boundsMode}", 10);
						if(ActiveObj != null)
							text($"Active object: {ActiveObj.Name}", 10);
						else if(ActiveLE != null)
						{
							text($"Active object: LandEntry {Scene.geometry.IndexOf(ActiveLE)}", 10);
						}
						else
							text($"Active object: NULL", 10);
						break;
				}

				g.Flush();
			}

			Canvas.Draw(_debugPanel);
		}
	}
}
