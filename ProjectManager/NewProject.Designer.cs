﻿namespace ProjectManager
{
    partial class NewProject
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewProject));
			this.SADXPCButton = new System.Windows.Forms.RadioButton();
			this.SA2RadioButton = new System.Windows.Forms.RadioButton();
			this.NextButton = new System.Windows.Forms.Button();
			this.BackButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.ProjectNameBox = new System.Windows.Forms.TextBox();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.AuthorLabel = new System.Windows.Forms.Label();
			this.AuthorTextBox = new System.Windows.Forms.TextBox();
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.DescriptionTextBox = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// SADXPCButton
			// 
			this.SADXPCButton.AutoSize = true;
			this.SADXPCButton.Location = new System.Drawing.Point(6, 19);
			this.SADXPCButton.Name = "SADXPCButton";
			this.SADXPCButton.Size = new System.Drawing.Size(71, 17);
			this.SADXPCButton.TabIndex = 0;
			this.SADXPCButton.TabStop = true;
			this.SADXPCButton.Text = "SADX PC";
			this.SADXPCButton.UseVisualStyleBackColor = true;
			this.SADXPCButton.CheckedChanged += new System.EventHandler(this.SADXPCButton_CheckedChanged);
			// 
			// SA2RadioButton
			// 
			this.SA2RadioButton.AutoSize = true;
			this.SA2RadioButton.Location = new System.Drawing.Point(6, 42);
			this.SA2RadioButton.Name = "SA2RadioButton";
			this.SA2RadioButton.Size = new System.Drawing.Size(62, 17);
			this.SA2RadioButton.TabIndex = 1;
			this.SA2RadioButton.TabStop = true;
			this.SA2RadioButton.Text = "SA2 PC";
			this.SA2RadioButton.UseVisualStyleBackColor = true;
			this.SA2RadioButton.CheckedChanged += new System.EventHandler(this.SA2RadioButton_CheckedChanged);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NextButton.Location = new System.Drawing.Point(334, 226);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = new System.Drawing.Size(75, 23);
			this.NextButton.TabIndex = 5;
			this.NextButton.Text = "Create Project";
			this.NextButton.UseVisualStyleBackColor = true;
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// BackButton
			// 
			this.BackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BackButton.Location = new System.Drawing.Point(8, 226);
			this.BackButton.Name = "BackButton";
			this.BackButton.Size = new System.Drawing.Size(75, 23);
			this.BackButton.TabIndex = 4;
			this.BackButton.Text = "Cancel";
			this.BackButton.UseVisualStyleBackColor = true;
			this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(5, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(71, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Project Name";
			// 
			// ProjectNameBox
			// 
			this.ProjectNameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProjectNameBox.Location = new System.Drawing.Point(8, 27);
			this.ProjectNameBox.Name = "ProjectNameBox";
			this.ProjectNameBox.Size = new System.Drawing.Size(401, 20);
			this.ProjectNameBox.TabIndex = 0;
			this.ProjectNameBox.TextChanged += new System.EventHandler(this.ProjectNameBox_TextChanged);
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.SADXPCButton);
			this.groupBox1.Controls.Add(this.SA2RadioButton);
			this.groupBox1.Location = new System.Drawing.Point(11, 52);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(105, 138);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "GameType";
			// 
			// AuthorLabel
			// 
			this.AuthorLabel.AutoSize = true;
			this.AuthorLabel.Location = new System.Drawing.Point(122, 54);
			this.AuthorLabel.Name = "AuthorLabel";
			this.AuthorLabel.Size = new System.Drawing.Size(38, 13);
			this.AuthorLabel.TabIndex = 7;
			this.AuthorLabel.Text = "Author";
			// 
			// AuthorTextBox
			// 
			this.AuthorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AuthorTextBox.Location = new System.Drawing.Point(125, 70);
			this.AuthorTextBox.Name = "AuthorTextBox";
			this.AuthorTextBox.Size = new System.Drawing.Size(284, 20);
			this.AuthorTextBox.TabIndex = 2;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.AutoSize = true;
			this.DescriptionLabel.Location = new System.Drawing.Point(122, 106);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = new System.Drawing.Size(57, 13);
			this.DescriptionLabel.TabIndex = 9;
			this.DescriptionLabel.Text = "Descripion";
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionTextBox.Location = new System.Drawing.Point(128, 122);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = new System.Drawing.Size(281, 98);
			this.DescriptionTextBox.TabIndex = 3;
			// 
			// NewProject
			// 
			this.AcceptButton = this.NextButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(421, 261);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.AuthorTextBox);
			this.Controls.Add(this.AuthorLabel);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.ProjectNameBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.BackButton);
			this.Controls.Add(this.NextButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "NewProject";
			this.Text = "New Project";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewProject_FormClosing);
			this.Shown += new System.EventHandler(this.NewProject_Shown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton SADXPCButton;
        private System.Windows.Forms.RadioButton SA2RadioButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ProjectNameBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label AuthorLabel;
        private System.Windows.Forms.TextBox AuthorTextBox;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.TextBox DescriptionTextBox;
    }
}