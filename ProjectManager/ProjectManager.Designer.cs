﻿namespace ProjectManager
{
    partial class ProjectManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectManager));
			this.NewProjectButton = new System.Windows.Forms.Button();
			this.OpenProjectButton = new System.Windows.Forms.Button();
			this.ConfigButton = new System.Windows.Forms.Button();
			this.SplitToolsButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// NewProjectButton
			// 
			this.NewProjectButton.Location = new System.Drawing.Point(25, 18);
			this.NewProjectButton.Name = "NewProjectButton";
			this.NewProjectButton.Size = new System.Drawing.Size(360, 55);
			this.NewProjectButton.TabIndex = 0;
			this.NewProjectButton.Text = "New Project";
			this.NewProjectButton.UseVisualStyleBackColor = true;
			this.NewProjectButton.Click += new System.EventHandler(this.NewProjectButton_Click);
			// 
			// OpenProjectButton
			// 
			this.OpenProjectButton.Location = new System.Drawing.Point(25, 88);
			this.OpenProjectButton.Name = "OpenProjectButton";
			this.OpenProjectButton.Size = new System.Drawing.Size(360, 55);
			this.OpenProjectButton.TabIndex = 1;
			this.OpenProjectButton.Text = "Open Project";
			this.OpenProjectButton.UseVisualStyleBackColor = true;
			this.OpenProjectButton.Click += new System.EventHandler(this.OpenProjectButton_Click);
			// 
			// ConfigButton
			// 
			this.ConfigButton.Location = new System.Drawing.Point(25, 159);
			this.ConfigButton.Name = "ConfigButton";
			this.ConfigButton.Size = new System.Drawing.Size(360, 23);
			this.ConfigButton.TabIndex = 2;
			this.ConfigButton.Text = "Config";
			this.ConfigButton.UseVisualStyleBackColor = true;
			this.ConfigButton.Click += new System.EventHandler(this.ConfigButton_Click);
			// 
			// SplitToolsButton
			// 
			this.SplitToolsButton.Location = new System.Drawing.Point(25, 203);
			this.SplitToolsButton.Name = "SplitToolsButton";
			this.SplitToolsButton.Size = new System.Drawing.Size(360, 23);
			this.SplitToolsButton.TabIndex = 3;
			this.SplitToolsButton.Text = "Manual Split Tools";
			this.SplitToolsButton.UseVisualStyleBackColor = true;
			this.SplitToolsButton.Click += new System.EventHandler(this.SplitToolsButton_Click);
			// 
			// ProjectManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(413, 246);
			this.Controls.Add(this.SplitToolsButton);
			this.Controls.Add(this.ConfigButton);
			this.Controls.Add(this.OpenProjectButton);
			this.Controls.Add(this.NewProjectButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ProjectManager";
			this.Text = "Project Manger";
			this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button NewProjectButton;
        private System.Windows.Forms.Button OpenProjectButton;
        private System.Windows.Forms.Button ConfigButton;
        private System.Windows.Forms.Button SplitToolsButton;
    }
}