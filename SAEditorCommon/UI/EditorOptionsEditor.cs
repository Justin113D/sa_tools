﻿using System;
using System.Windows.Forms;
using SharpDX.Direct3D9;
using SonicRetro.SAModel.Direct3D;

namespace SonicRetro.SAModel.SAEditorCommon.UI
{
	public partial class EditorOptionsEditor : Form
	{
		public Action CustomizeKeybindsCommand;
		public Action ResetDefaultKeybindsCommand;

		public delegate void FormUpdatedHandler();
		public event FormUpdatedHandler FormUpdated;

		private EditorCamera camera;
		private ToolTip toolTip = new ToolTip();

		public EditorOptionsEditor(EditorCamera camera, bool setdist_enabled_a, bool leveldist_enabled_a)
		{
			InitializeComponent();
			this.camera = camera;
			drawDistSlider.Value = (int)EditorOptions.RenderDrawDistance;
			fillModeDropDown.SelectedIndex = (int)EditorOptions.RenderFillMode - 1;
			cullModeDropdown.SelectedIndex = (int)EditorOptions.RenderCullMode - 1;
			if (setdist_enabled_a)
			{
				setDrawDistLabel.Enabled = true;
				setDrawDistSlider.Enabled = true;

			}
			if (leveldist_enabled_a)
			{
				levelDrawDistLabel.Enabled = true;
				levelDrawDistSlider.Enabled = true;
			}
			drawDistLabel.Text = String.Format("General: {0}", drawDistSlider.Value);
			levelDrawDistLabel.Text = String.Format("Level Geometry: {0}", levelDrawDistSlider.Value); ;
			setDrawDistLabel.Text = String.Format("SET/CAM Items: {0}", setDrawDistSlider.Value);
			toolTip.SetToolTip(fullBrightCheck, "If the scene's lighting is making it hard to work, use this to temporarily set the lighting to flat-white.");
			fullBrightCheck.Checked = EditorOptions.OverrideLighting;
		}

		private void drawDistSlider_Scroll(object sender, EventArgs e)
		{
			if (drawDistSlider.Value < levelDrawDistSlider.Value) levelDrawDistSlider.Value = drawDistSlider.Value;
			if (drawDistSlider.Value < setDrawDistSlider.Value) setDrawDistSlider.Value = drawDistSlider.Value;
			drawDistLabel.Text = String.Format("General: {0}", drawDistSlider.Value);
			EditorOptions.RenderDrawDistance = drawDistSlider.Value;
			camera.DrawDistance = EditorOptions.RenderDrawDistance;
			FormUpdated();
		}

		private void levelDrawDistSlider_Scroll(object sender, EventArgs e)
		{
			if (drawDistSlider.Value < levelDrawDistSlider.Value) drawDistSlider.Value = levelDrawDistSlider.Value;
			levelDrawDistLabel.Text = String.Format("Level Geometry: {0}", levelDrawDistSlider.Value);
			EditorOptions.LevelDrawDistance = levelDrawDistSlider.Value;
			FormUpdated();
		}

		private void setDrawDistSlider_Scroll(object sender, EventArgs e)
		{
			if (drawDistSlider.Value < setDrawDistSlider.Value) drawDistSlider.Value = setDrawDistSlider.Value;
			setDrawDistLabel.Text = String.Format("SET/CAM Items: {0}", setDrawDistSlider.Value);
			EditorOptions.SetItemDrawDistance = setDrawDistSlider.Value;
			FormUpdated();
		}

		private void fillModeDropDown_SelectionChangeCommitted(object sender, EventArgs e)
		{
			EditorOptions.RenderFillMode = (FillMode)fillModeDropDown.SelectedIndex + 1;
			FormUpdated();
		}

		private void cullModeDropdown_SelectionChangeCommitted(object sender, EventArgs e)
		{
			EditorOptions.RenderCullMode = (Cull)cullModeDropdown.SelectedIndex + 1;
			FormUpdated();
		}

		private void doneButton_Click(object sender, EventArgs e)
		{
			FormUpdated();
			Close();
		}

		private void fullBrightCheck_Click(object sender, EventArgs e)
		{
			EditorOptions.OverrideLighting = fullBrightCheck.Checked;
			FormUpdated();
		}

		private void KeyboardShortcutButton_Click(object sender, EventArgs e)
		{
			CustomizeKeybindsCommand.Invoke();
		}

		private void EditorOptionsEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}

		private void ResetDefaultKeybindButton_Click(object sender, EventArgs e)
		{
			ResetDefaultKeybindsCommand.Invoke();
		}
	}
}
