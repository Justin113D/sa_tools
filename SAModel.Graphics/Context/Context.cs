using SonicRetro.SAModel.Structs;
using Color = SonicRetro.SAModel.Structs.Color;
using System.Drawing;
using System.Windows.Interop;
using SonicRetro.SAModel.Graphics.UI;

namespace SonicRetro.SAModel.Graphics
{
	/// <summary>
	/// Rendering context base class
	/// </summary>
	public abstract class Context
	{
		public static Context Focused { get; private set; }

		/// <summary>
		/// Positiona and resolution of the context
		/// </summary>
		protected Rectangle _screen;

		/// <summary>
		/// Center of the screen, 
		/// </summary>
		public Point Center { get; protected set; }

		/// <summary>
		/// Whether the context was focused in the last update
		/// </summary>
		protected bool _wasFocused;

		public Color BackgroundColor { get; protected set; }

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

		public Canvas Canvas { get; }

		/// <summary>
		/// Creates a new render context
		/// </summary>
		/// <param name="inputUpdater"></param>
		public Context(Rectangle screen, Camera camera, Canvas canvas, InputUpdater inputUpdater)
		{
			_screen = screen;
			Camera = camera;
			Scene = new Scene(camera);
			Canvas = canvas;
			BackgroundColor = new Color(0x60, 0x60, 0x60);

			Input.Init(inputUpdater);
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
		/// Resets focus
		/// </summary>
		public static void ResetFocus() => Focused = null;

		/// <summary>
		/// Focuses this context
		/// </summary>
		public void Focus() => Focused = this;

		/// <summary>
		/// Gets called when the window gets moved or resized
		/// </summary>
		/// <param name="resize">Whether the screen is being resized</param>
		protected virtual void UpdateScreen(bool resize)
		{
			int x = _screen.X + _screen.Width / 2;
			int y = _screen.Y + _screen.Height / 2;
			Center = new Point(x, y);

			if(resize)
			{
				Camera.Aspect = _screen.Width / (float)_screen.Height;
			}
		}

		public void Update(double delta)
		{
			if(Focused == this || _wasFocused)
			{
				Input.Update(_wasFocused);
			}

			ContextUpdate(delta);

			Scene.Update(delta);

			_wasFocused = Focused == this;
		}

		protected abstract void ContextUpdate(double delta);

		public void Render()
		{
			ContextRender();
			Canvas.Render(_screen.Width, _screen.Height);
		}

		protected abstract void ContextRender();
	}
}
