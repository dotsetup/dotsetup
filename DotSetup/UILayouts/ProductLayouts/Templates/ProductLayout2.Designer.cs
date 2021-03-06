﻿using System.Drawing;
using DotSetup.UILayouts.UIComponents;

namespace DotSetup.UILayouts.ControlLayout.Templates
{
    partial class ProductLayout2
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlLayout = new System.Windows.Forms.Panel();
            this.txtDisclaimer = new RichTextBoxEx();
            this.imgBackground = new PanelEx();
            this.imgOptional = new PictureBoxEx();
            this.txtSmOptInY = new System.Windows.Forms.RadioButton();
            this.txtSmOptInN = new System.Windows.Forms.RadioButton();
            this.txtSmOptIn = new RichTextBoxEx();
            this.imgSmOpInBg = new PictureBoxEx();
            this.txtOptIn = new System.Windows.Forms.CheckBox();
            this.pnlDarkenOverlay = new System.Windows.Forms.Panel();
            this.txtTitle = new RichTextBoxEx();
            this.txtDescription = new RichTextBoxEx();
            this.pnlLayout.SuspendLayout();
            this.imgBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgOptional)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSmOpInBg)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlLayout
            // 
            this.pnlLayout.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayout.Controls.Add(this.txtDisclaimer);
            this.pnlLayout.Controls.Add(this.imgBackground);
            this.pnlLayout.Controls.Add(this.txtSmOptIn);
            this.pnlLayout.Controls.Add(this.imgSmOpInBg);
            this.pnlLayout.Controls.Add(this.txtOptIn);
            this.pnlLayout.Controls.Add(this.pnlDarkenOverlay);
            this.pnlLayout.Controls.Add(this.txtTitle);
            this.pnlLayout.Controls.Add(this.txtDescription);
            this.pnlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLayout.Location = new System.Drawing.Point(0, 0);
            this.pnlLayout.Margin = new System.Windows.Forms.Padding(0);
            this.pnlLayout.Name = "pnlLayout";
            this.pnlLayout.Size = new System.Drawing.Size(600, 320);
            this.pnlLayout.TabIndex = 11;
            // 
            // txtDisclaimer
            // 
            this.txtDisclaimer.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtDisclaimer.BackColor = System.Drawing.Color.White;
            this.txtDisclaimer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDisclaimer.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtDisclaimer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtDisclaimer.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.txtDisclaimer.ForeColor = System.Drawing.Color.White;
            this.txtDisclaimer.LineSpacing = 210;
            this.txtDisclaimer.Location = new System.Drawing.Point(0, 263);
            this.txtDisclaimer.Name = "txtDisclaimer";
            this.txtDisclaimer.Padding = 2;
            this.txtDisclaimer.ReadOnly = true;
            this.txtDisclaimer.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtDisclaimer.Size = new System.Drawing.Size(600, 57);
            this.txtDisclaimer.TabIndex = 11;
            this.txtDisclaimer.Text = "";
            // 
            // imgBackground
            // 
            this.imgBackground.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.imgBackground.Controls.Add(this.imgOptional);
            this.imgBackground.Controls.Add(this.txtSmOptInY);
            this.imgBackground.Controls.Add(this.txtSmOptInN);
            this.imgBackground.Dock = System.Windows.Forms.DockStyle.Top;
            this.imgBackground.Location = new System.Drawing.Point(0, 0);
            this.imgBackground.Margin = new System.Windows.Forms.Padding(0);
            this.imgBackground.Name = "imgBackground";
            this.imgBackground.Size = new System.Drawing.Size(600, 260);
            this.imgBackground.TabIndex = 9;
            // 
            // imgOptional
            // 
            this.imgOptional.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.imgOptional.BackColor = System.Drawing.Color.Transparent;
            this.imgOptional.Location = new System.Drawing.Point(380, 0);
            this.imgOptional.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.imgOptional.Name = "imgOptional";
            this.imgOptional.Size = new System.Drawing.Size(220, 21);
            this.imgOptional.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgOptional.TabIndex = 27;
            this.imgOptional.TabStop = false;
            // 
            // txtSmOptInY
            // 
            this.txtSmOptInY.AutoSize = true;
            this.txtSmOptInY.Font = new System.Drawing.Font("Arial", 9.25F, System.Drawing.FontStyle.Bold);
            this.txtSmOptInY.Location = new System.Drawing.Point(55, 197);
            this.txtSmOptInY.Name = "txtSmOptInY";
            this.txtSmOptInY.Size = new System.Drawing.Size(107, 20);
            this.txtSmOptInY.TabIndex = 23;
            this.txtSmOptInY.TabStop = true;
            this.txtSmOptInY.Text = "radioButton1";
            this.txtSmOptInY.UseVisualStyleBackColor = true;
            this.txtSmOptInY.Visible = false;
            // 
            // txtSmOptInN
            // 
            this.txtSmOptInN.AutoSize = true;
            this.txtSmOptInN.Font = new System.Drawing.Font("Arial", 9.25F, System.Drawing.FontStyle.Bold);
            this.txtSmOptInN.Location = new System.Drawing.Point(55, 223);
            this.txtSmOptInN.Name = "txtSmOptInN";
            this.txtSmOptInN.Size = new System.Drawing.Size(107, 20);
            this.txtSmOptInN.TabIndex = 22;
            this.txtSmOptInN.TabStop = true;
            this.txtSmOptInN.Text = "radioButton2";
            this.txtSmOptInN.UseVisualStyleBackColor = true;
            this.txtSmOptInN.Visible = false;
            // 
            // txtSmOptIn
            // 
            this.txtSmOptIn.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtSmOptIn.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSmOptIn.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.txtSmOptIn.LineSpacing = 300;
            this.txtSmOptIn.Location = new System.Drawing.Point(127, 217);
            this.txtSmOptIn.Name = "txtSmOptIn";
            this.txtSmOptIn.ReadOnly = true;
            this.txtSmOptIn.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtSmOptIn.Size = new System.Drawing.Size(386, 38);
            this.txtSmOptIn.TabIndex = 21;
            this.txtSmOptIn.Text = "";
            this.txtSmOptIn.Visible = false;
            // 
            // imgSmOpInBg
            // 
            this.imgSmOpInBg.Location = new System.Drawing.Point(46, 186);
            this.imgSmOpInBg.Name = "imgSmOpInBg";
            this.imgSmOpInBg.Size = new System.Drawing.Size(469, 73);
            this.imgSmOpInBg.TabIndex = 18;
            this.imgSmOpInBg.TabStop = false;
            this.imgSmOpInBg.Visible = false;
            // 
            // txtOptIn
            // 
            this.txtOptIn.AutoSize = true;
            this.txtOptIn.Location = new System.Drawing.Point(3, 200);
            this.txtOptIn.Name = "txtOptIn";
            this.txtOptIn.Size = new System.Drawing.Size(80, 17);
            this.txtOptIn.TabIndex = 15;
            this.txtOptIn.Text = "checkBox1";
            this.txtOptIn.UseVisualStyleBackColor = true;
            this.txtOptIn.Visible = false;
            // 
            // pnlDarkenOverlay
            // 
            this.pnlDarkenOverlay.Location = new System.Drawing.Point(0, 0);
            this.pnlDarkenOverlay.Name = "pnlDarkenOverlay";
            this.pnlDarkenOverlay.Size = new System.Drawing.Size(560, 260);
            this.pnlDarkenOverlay.TabIndex = 24;
            this.pnlDarkenOverlay.Visible = false;
            // 
            // txtTitle
            // 
            this.txtTitle.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTitle.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Bold);
            this.txtTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(173)))));
            this.txtTitle.LineSpacing = 300;
            this.txtTitle.Location = new System.Drawing.Point(3, 63);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.ReadOnly = true;
            this.txtTitle.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtTitle.Size = new System.Drawing.Size(271, 38);
            this.txtTitle.TabIndex = 25;
            this.txtTitle.Text = "";
            // 
            // txtDescription
            // 
            this.txtDescription.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescription.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.LineSpacing = 300;
            this.txtDescription.Location = new System.Drawing.Point(3, 89);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtDescription.Size = new System.Drawing.Size(358, 117);
            this.txtDescription.TabIndex = 12;
            this.txtDescription.Text = "";
            // 
            // ProductLayout2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlLayout);
            this.Name = "ProductLayout2";
            this.Size = new System.Drawing.Size(600, 320);
            this.Load += new System.EventHandler(this.ProductLayout2_Load);
            this.pnlLayout.ResumeLayout(false);
            this.pnlLayout.PerformLayout();
            this.imgBackground.ResumeLayout(false);
            this.imgBackground.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgOptional)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSmOpInBg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLayout;
        private PanelEx imgBackground;
        private RichTextBoxEx txtDisclaimer;
        private RichTextBoxEx txtDescription;
        private System.Windows.Forms.CheckBox txtOptIn;
        private PictureBoxEx imgSmOpInBg;
        private System.Windows.Forms.RadioButton txtSmOptInY;
        private System.Windows.Forms.RadioButton txtSmOptInN;
        private RichTextBoxEx txtSmOptIn;
        private System.Windows.Forms.Panel pnlDarkenOverlay;
        private RichTextBoxEx txtTitle;
        private PictureBoxEx imgOptional;
    }
}
