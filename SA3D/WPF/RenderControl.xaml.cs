using SonicRetro.SAModel.Graphics;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace SonicRetro.SA3D.WPF.Viewmodel
{
	/// <summary>
	/// Interaction logic for RenderControl.xaml
	/// </summary>
	public partial class RenderControl : UserControl
	{
		private readonly Context _renderContext;

		public RenderControl(Context renderContext)
		{
			InitializeComponent();

			_renderContext = renderContext;

			var window = Window.GetWindow(this);
			var baseHandle = window is null ? IntPtr.Zero : new WindowInteropHelper(window).Handle;
			var _hwnd = new HwndSource(0, 0, 0, 0, 0, "GLWpfControl", baseHandle);
			Render.Content = _renderContext.AsControl(_hwnd);
		}

		public void UpdateFocus()
		{
			_renderContext.Focused = IsMouseOver;
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			_renderContext.Resolution = new System.Drawing.Size((int)RenderSize.Width, (int)RenderSize.Height);
		}
	}
}
