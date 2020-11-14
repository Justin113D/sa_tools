﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SonicRetro.SAModel.Direct3D.TextureSystem;

namespace SonicRetro.SAModel.SAEditorCommon.UI
{
	public partial class MaterialEditor : Form
	{
		#region Events

		public delegate void FormUpdatedHandler(object sender, EventArgs e);
		public event FormUpdatedHandler FormUpdated;

		#endregion

		private readonly List<NJS_MATERIAL> materials;
		private readonly BMPInfo[] textures;

		public MaterialEditor(List<NJS_MATERIAL> mats, BMPInfo[] textures)
		{
			materials = mats;
			this.textures = textures;
			InitializeComponent();
		}

		private void MaterialEditor_Load(object sender, EventArgs e)
		{
			for (int i = 0; i < materials.Count; i++)
				comboMaterial.Items.Add(i);
			if (comboMaterial.Items.Count > 0)
				comboMaterial.SelectedIndex = 0;

			SetControls(comboMaterial.SelectedIndex);
		}

		/// <summary>
		/// Populates the form with data from a material index.
		/// </summary>
		/// <param name="index">Index of the material to use.</param>
		private void SetControls(int index)
		{
			// setting general
			diffuseColorBox.BackColor = materials[index].DiffuseColor;
			alphaDiffuseNumeric.Value = materials[index].DiffuseColor.A;
			specColorBox.BackColor = materials[index].SpecularColor;
			alphaSpecularNumeric.Value = materials[index].SpecularColor.A;
			if(textures != null && materials[index].TextureID < textures.Length) textureBox.Image = textures[materials[index].TextureID].Image;
			exponentTextBox.Text = materials[index].Exponent.ToString();
			filterModeDropDown.SelectedIndex = (int)materials[index].FilterMode;
			srcAlphaCombo.SelectedIndex = (int)materials[index].SourceAlpha;
			dstAlphaCombo.SelectedIndex = (int)materials[index].DestinationAlpha;

			// setting flags
			pickStatusCheck.Checked = materials[index].PickStatus;
			superSampleCheck.Checked = materials[index].SuperSample;
			clampUCheck.Checked = materials[index].ClampU;
			clampVCheck.Checked = materials[index].ClampV;
			flipUCheck.Checked = materials[index].FlipU;
			flipVCheck.Checked = materials[index].FlipV;
			ignoreSpecCheck.Checked = materials[index].IgnoreSpecular;
			useAlphaCheck.Checked = materials[index].UseAlpha;
			useTextureCheck.Checked = materials[index].UseTexture;
			envMapCheck.Checked = materials[index].EnvironmentMap;
			doubleSideCheck.Checked = materials[index].DoubleSided;
			flatShadeCheck.Checked = materials[index].FlatShading;
			ignoreLightCheck.Checked = materials[index].IgnoreLighting;
			userFlagsNumeric.Value = materials[index].UserFlags;

			DisplayFlags(index);
		}

		private void comboMaterial_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboMaterial.SelectedIndex > -1)
				SetControls(comboMaterial.SelectedIndex);
		}

		#region General Control Event Methods

		private void textureBox_Click(object sender, EventArgs e)
		{
			if (textures == null)
			{
				MessageBox.Show("No textures loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			using (TexturePicker texPicker = new TexturePicker(textures, materials[comboMaterial.SelectedIndex].TextureID))
			{
				if (texPicker.ShowDialog(this) == DialogResult.OK)
				{
					materials[comboMaterial.SelectedIndex].TextureID = texPicker.SelectedValue;
					textureBox.Image = textures[materials[comboMaterial.SelectedIndex].TextureID].Image;

					RaiseFormUpdated();
				}
			}
		}

		private void diffuseColorBox_Click(object sender, EventArgs e)
		{
			colorDialog.Color = materials[comboMaterial.SelectedIndex].DiffuseColor;
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				materials[comboMaterial.SelectedIndex].DiffuseColor = Color.FromArgb((int)alphaDiffuseNumeric.Value, colorDialog.Color);
				diffuseColorBox.BackColor = materials[comboMaterial.SelectedIndex].DiffuseColor;
				RaiseFormUpdated();
			}
		}

		private void specColorBox_Click(object sender, EventArgs e)
		{
			colorDialog.Color = materials[comboMaterial.SelectedIndex].SpecularColor;
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				materials[comboMaterial.SelectedIndex].SpecularColor = Color.FromArgb((int)alphaSpecularNumeric.Value, colorDialog.Color);
				specColorBox.BackColor = materials[comboMaterial.SelectedIndex].SpecularColor;
				RaiseFormUpdated();
			}
		}

		private void doneButton_Click(object sender, EventArgs e)
		{
			ValidateExponent();
			Close();
		}

		private void exponentTextBox_Leave(object sender, EventArgs e)
		{
			ValidateExponent();
			RaiseFormUpdated();
		}

		private void ValidateExponent()
		{
			// check to see if exponent can be parsed

			if (!float.TryParse(exponentTextBox.Text, out float expParse))
			{
				MessageBox.Show("Specular exponent was invalid - setting to 10");
				materials[comboMaterial.SelectedIndex].Exponent = 10;
			}
			else
			{
				materials[comboMaterial.SelectedIndex].Exponent = expParse;
			}
		}

		private void alphaDiffuseNumeric_ValueChanged(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].DiffuseColor = Color.FromArgb((int)alphaDiffuseNumeric.Value, materials[comboMaterial.SelectedIndex].DiffuseColor);
			diffuseColorBox.BackColor = materials[comboMaterial.SelectedIndex].DiffuseColor;
			RaiseFormUpdated();
		}

		#endregion

		#region Flag Check Event Methods

		private void pickStatusCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].PickStatus = pickStatusCheck.Checked;
			RaiseFormUpdated();
		}

		private void superSampleCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].SuperSample = superSampleCheck.Checked;
			RaiseFormUpdated();
		}

		private void clampUCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].ClampU = clampUCheck.Checked;
			RaiseFormUpdated();
		}

		private void clampVCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].ClampV = clampVCheck.Checked;
			RaiseFormUpdated();
		}

		private void flipUCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].FlipU = flipUCheck.Checked;
			RaiseFormUpdated();
		}

		private void flipVCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].FlipV = flipVCheck.Checked;
			RaiseFormUpdated();
		}

		private void ignoreSpecCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].IgnoreSpecular = ignoreSpecCheck.Checked;
			RaiseFormUpdated();
		}

		private void useAlphaCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].UseAlpha = useAlphaCheck.Checked;
			RaiseFormUpdated();
		}

		private void useTextureCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].UseTexture = useTextureCheck.Checked;
			RaiseFormUpdated();
		}

		private void envMapCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].EnvironmentMap = envMapCheck.Checked;
			RaiseFormUpdated();
		}

		private void doubleSideCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].DoubleSided = doubleSideCheck.Checked;
			RaiseFormUpdated();
		}

		private void flatShadeCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].FlatShading = flatShadeCheck.Checked;
			RaiseFormUpdated();
		}

		private void ignoreLightCheck_Click(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].IgnoreLighting = ignoreLightCheck.Checked;
			RaiseFormUpdated();
		}

		private void userFlagsNumeric_ValueChanged(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].UserFlags = (byte)userFlagsNumeric.Value;
			RaiseFormUpdated();
		}

		private void filterModeDropDown_SelectionChangeCommitted(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].FilterMode = (FilterMode)filterModeDropDown.SelectedIndex;
			RaiseFormUpdated();
		}

		private void srcAlphaCombo_SelectionChangeCommitted(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].SourceAlpha = (AlphaInstruction)srcAlphaCombo.SelectedIndex;
			RaiseFormUpdated();
		}

		private void dstAlphaCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].DestinationAlpha = (AlphaInstruction)dstAlphaCombo.SelectedIndex;
			RaiseFormUpdated();
		}
		
		#endregion

		private void RaiseFormUpdated()
		{
			FormUpdated?.Invoke(this, EventArgs.Empty);

			DisplayFlags(comboMaterial.SelectedIndex);
		}

		private void DisplayFlags(int index)
		{
			labelFlags.Text = String.Format("{0:X8}", materials[index].Flags);
		}

		private void onKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Tab:
					if (e.Control)
					{
						int index = comboMaterial.SelectedIndex;
						int count = comboMaterial.Items.Count;
						
						if (e.Shift)
							--index;
						else
							++index;

						index = (index < 0) ? (index + count) : (index % count);
						comboMaterial.SelectedIndex = index;
					}
					break;
			}
		}

		private void alphaSpecularNumeric_ValueChanged(object sender, EventArgs e)
		{
			materials[comboMaterial.SelectedIndex].SpecularColor = Color.FromArgb((int)alphaSpecularNumeric.Value, materials[comboMaterial.SelectedIndex].SpecularColor);
			specColorBox.BackColor = materials[comboMaterial.SelectedIndex].SpecularColor;
			RaiseFormUpdated();
		}
	}
}