using Microsoft.Win32;
using SonicRetro.SA3D.WPF.Viewmodel;
using SonicRetro.SA3D.WPF.ViewModel.Base;
using SonicRetro.SAModel.Graphics;
using SonicRetro.SAModel.Graphics.OpenGL;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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

		private readonly Context _renderContext;

		/// <summary>
		/// Main control that is displayed below the menu
		/// </summary>
		public UserControl MainControl { get; private set; }

		/// <summary>
		/// Render control used to display models (and other various things)
		/// </summary>
		public RenderControl RenderControl { get; }

		public RelayCommand OpenFileRC { get; }

		public MainViewModel()
		{
			_renderContext = new GLContext(default);
			RenderControl = new RenderControl(_renderContext);
			MainControl = RenderControl;
			OpenFileRC = new RelayCommand(OpenFile);
		}

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
						_renderContext.Scene.LoadModelFile(mdlFile);
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
						_renderContext.Scene.LoadLandtable(ltbl);
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
