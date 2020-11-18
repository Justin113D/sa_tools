using Microsoft.Win32;
using SonicRetro.SA3D.WPF.ViewModel.Base;
using SonicRetro.SAModel.Graphics;
using SonicRetro.SAModel.Graphics.OpenGL;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace SonicRetro.SA3D.WPF.ViewModel
{
	enum Mode
	{
		Model,
		Level,
		ProjectSA1,
		ProjectSA2
	}

	/// <summary>
	/// Main view model used to control the entire application
	/// </summary>
	public class MainViewModel : BaseViewModel
	{
		/// <summary>
		/// Application mode
		/// </summary>
		private Mode _applicationMode;

		private MainWindow _window;

		/// <summary>
		/// Render context being displayed
		/// </summary>
		public DebugContext RenderContext { get; }

		/// <summary>
		/// Rendercontrol output
		/// </summary>
		public FrameworkElement RenderControl { get; }

		/// <summary>
		/// Gets called on mouse down <br/> 
		/// Sets the context accordingly to focused/not focused
		/// </summary>
		public RelayCommand FocusWindow { get; }

		/// <summary>
		/// Gets called when <see cref="RenderControl"/>s size changes <br/> 
		/// Updates the contexts resolution
		/// </summary>
		public RelayCommand ResizeRender { get; }

		public RelayCommand SelectionChangedCommand { get; }

		public RelayCommand OpenFileRC { get; }

		public NJObjectTreeVM NJObjectTreeVM { get; }

		public MainViewModel(MainWindow window)
		{
			RenderContext = new GLDebugContext(default);

			NJObjectTreeVM = new NJObjectTreeVM(this);
			FocusWindow = new RelayCommand(Focus);
			ResizeRender = new RelayCommand(Resize);
			OpenFileRC = new RelayCommand(OpenFile);

			_window = window;
			var _hwnd = new HwndSource(0, 0, 0, 0, 0, "RenderControl", new WindowInteropHelper(window).Handle);

			// TODO once graphics.DX exists and works, we gotta make it switchable
			RenderControl = RenderContext.AsControl(_hwnd);

		}

		/// <summary>
		/// Focuses the context or not
		/// </summary>
		public void Focus()
		{
			if(_window.IsMouseOver)
				RenderContext.Focus();
			else
				Context.ResetFocus();
		}

		/// <summary>
		/// Passes the rendercontrol resolution to the context
		/// </summary>
		public void Resize()
		{
			RenderContext.Resolution = new System.Drawing.Size((int)_window.Render.ActualWidth, (int)_window.Render.ActualHeight);
		}

		/// <summary>
		/// Opens and loads a model/level file
		/// </summary>
		private void OpenFile()
		{
			OpenFileDialog ofd = new OpenFileDialog()
			{
				Filter = "SA3D File(*.*mdl, *.nj, *.*lvl)|*.SA1MDL;*.SA2MDL;*.SA2BMDL;*.NJ;*.SA1LVL;*.SA2LVL;*.SA2BLVL"
			};

			if(ofd.ShowDialog() == true)
			{
				// reading the file indicator
				byte[] file = File.ReadAllBytes(ofd.FileName);
				try
				{
					var mdlFile = SAModel.ObjData.ModelFile.Read(file, ofd.FileName);
					if(mdlFile != null)
					{
						_applicationMode = Mode.Model;
						RenderContext.Scene.LoadModelFile(mdlFile);
						NJObjectTreeVM.Refresh();
						return;
					}
				}
				catch(Exception e)
				{
					MessageBox.Show("Error while reading model file!\n " + e.Message, e.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
				}

				try
				{
					var ltbl = SAModel.ObjData.LandTable.ReadFile(file);
					if(ltbl != null)
					{
						_applicationMode = Mode.Level;
						RenderContext.Scene.LoadLandtable(ltbl);
						return;
					}
				}
				catch (Exception e)
				{
					MessageBox.Show("Error while reading level file!\n " + e.Message, e.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
				}

				MessageBox.Show("File not in any valid format", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
