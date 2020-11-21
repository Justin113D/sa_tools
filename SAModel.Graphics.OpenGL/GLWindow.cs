using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SonicRetro.SAModel.Graphics;

namespace SonicRetro.SAModel.Graphics.OpenGL
{
	public class GLWindow : GameWindow
	{
		private readonly Context _context;

		private bool _showedCursor;

		public GLWindow(Context context, int width, int height) :
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
			_context.Resolution = ClientSize;
		}

		protected override void OnMove(EventArgs e)
		{
			base.OnMove(e);
			_context.Location = Location;
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
			_context.IsFocused = Focused;
			_context.Update((float)e.Time);
			if(_showedCursor == _context.Camera.Orbiting)
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
