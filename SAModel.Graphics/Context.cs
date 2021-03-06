﻿using SonicRetro.SAModel.Graphics.APIAccess;
using SonicRetro.SAModel.Graphics.UI;
using System.Drawing;
using System.Windows.Interop;
using Color = SonicRetro.SAModel.Structs.Color;

namespace SonicRetro.SAModel.Graphics
{
	/// <summary>
	/// Rendering context base class
	/// </summary>
	public class Context
	{
		#region Statics

		/// <summary>
		/// The focused context
		/// </summary>
		public static Context Focused { get; private set; }

		#endregion

		#region private fields

		/// <summary>
		/// Graphics API Access Object
		/// </summary>
		protected GAPIAccessObject _apiAccessObject;

		/// <summary>
		/// Whether the context was focused in the last update
		/// </summary>
		protected bool _wasFocused;

		private Point _screenCenter;

		/// <summary>
		/// Positiona and resolution of the context
		/// </summary>
		protected Rectangle _screen;

		/// <summary>
		/// see <see cref="BackgroundColor"/>
		/// </summary>
		private Color _backgroundColor;

		#endregion

		#region Public Properties

		/// <summary>
		/// Whether this window is in focus
		/// </summary>
		public bool IsFocused
		{
			get => Focused == this;
			set
			{
				if(value)
					Focused = this;
				else if(Focused == this)
					Focused = null;
			}
		}

		/// <summary>
		/// Polygon display handler
		/// </summary>
		public Material Material { get; }

		/// <summary>
		/// UI handler
		/// </summary>
		public Canvas Canvas { get; }

		/// <summary>
		/// Input handler
		/// </summary>
		public Input Input { get; }

		/// <summary>
		/// The camera of the scene
		/// </summary>
		public Camera Camera { get; }

		/// <summary>
		/// 3D scene to hold objects and geometry
		/// </summary>
		public Scene Scene { get; }

		/// <summary>
		/// Screen Rectangle (get only)
		/// </summary>
		public Rectangle Screen => _screen;

		/// <summary>
		/// The resolution of the context
		/// </summary>
		public Size Resolution
		{
			get => _screen.Size;
			set
			{
				_screen.Size = value;
				UpdateScreenCenter();
				Camera.Aspect = _screen.Width / (float)_screen.Height;
				_apiAccessObject.UpdateViewport(_screen, true);
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
				UpdateScreenCenter();
				_apiAccessObject.UpdateViewport(_screen, false);
			}
		}

		/// <summary>
		/// Center of the screen (useful for locking the mouse position)d
		/// </summary>
		public Point ScreenCenter => _screenCenter;

		/// <summary>
		/// Clearcolor
		/// </summary>
		public Color BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value;
				_apiAccessObject.UpdateBackgroundColor(_backgroundColor);
			}
		}

		#endregion

		/// <summary>
		/// Creates a new render context
		/// </summary>
		/// <param name="inputUpdater"></param>
		public Context(Rectangle screen, GAPIAccessObject apiAccessObject)
		{
			_apiAccessObject = apiAccessObject;

			_screen = screen;
			Camera = new Camera(screen.Width / (float)screen.Height, apiAccessObject);
			Material = new Material(apiAccessObject);
			Canvas = new Canvas(apiAccessObject);
			Input = new Input(apiAccessObject);
			Scene = new Scene(Camera);
			_backgroundColor = new Color(0x60, 0x60, 0x60);
		}

		/// <summary>
		/// Gets called when the window gets moved or resized
		/// </summary>
		/// <param name="resized">Whether the screen is being resized</param>
		private void UpdateScreenCenter()
		{
			int x = _screen.X + _screen.Width / 2;
			int y = _screen.Y + _screen.Height / 2;
			_screenCenter = new Point(x, y);
		}

		/// <summary>
		/// Starts the context as an independent window
		/// </summary>
		public void AsWindow()
			=> _apiAccessObject.AsWindow(this);

		/// <summary>
		/// Returns the context as a WPF control
		/// </summary>
		/// <param name="windowSource"></param>
		/// <returns></returns>
		public System.Windows.FrameworkElement AsControl(HwndSource windowSource)
			=> _apiAccessObject.AsControl(this, windowSource);

		/// <summary>
		/// Gets called when graphics are being initialized
		/// </summary>
		public virtual void GraphicsInit()
			=> _apiAccessObject.GraphicsInit(this);

		/// <summary>
		/// Gameplay logic update
		/// </summary>
		/// <param name="delta"></param>
		public void Update(double delta)
		{
			if(Focused == this || _wasFocused)
				Input.Update(_wasFocused);

			Scene.Update(delta);

			_wasFocused = Focused == this;
		}

		public virtual void Render()
		{
			_apiAccessObject.Render(this);
			Canvas.Render(_screen.Width, _screen.Height);
		}
	}
}
