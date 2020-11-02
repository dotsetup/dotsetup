namespace DotSetup
{
    partial class ProductLayout6
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductLayout6));
            this.pnlLayout = new System.Windows.Forms.Panel();
            this.txtDisclaimer = new DotSetup.RichTextBoxEx();
            this.txtDescription = new DotSetup.RichTextBoxEx();
            this.imgLogo = new DotSetup.PictureBoxEx();
            this.imgBackground = new DotSetup.PictureBoxEx();
            this.pnlLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgBackground)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlLayout
            // 
            this.pnlLayout.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayout.Controls.Add(this.txtDisclaimer);
            this.pnlLayout.Controls.Add(this.txtDescription);
            this.pnlLayout.Controls.Add(this.imgLogo);
            this.pnlLayout.Controls.Add(this.imgBackground);
            this.pnlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLayout.Location = new System.Drawing.Point(0, 0);
            this.pnlLayout.Name = "pnlLayout";
            this.pnlLayout.Size = new System.Drawing.Size(560, 260);
            this.pnlLayout.TabIndex = 11;
            // 
            // txtDisclaimer
            // 
            this.txtDisclaimer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDisclaimer.BackColor = System.Drawing.Color.White;
            this.txtDisclaimer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDisclaimer.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtDisclaimer.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.txtDisclaimer.ForeColor = System.Drawing.SystemColors.InfoText;
            this.txtDisclaimer.Location = new System.Drawing.Point(3, 225);
            this.txtDisclaimer.Name = "txtDisclaimer";
            this.txtDisclaimer.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtDisclaimer.Size = new System.Drawing.Size(554, 32);
            this.txtDisclaimer.TabIndex = 11;
            this.txtDisclaimer.Text = resources.GetString("txtDisclaimer.Text");
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.BackColor = System.Drawing.Color.White;
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescription.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDescription.ForeColor = System.Drawing.SystemColors.InfoText;
            this.txtDescription.Location = new System.Drawing.Point(121, 0);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtDescription.Size = new System.Drawing.Size(435, 41);
            this.txtDescription.TabIndex = 8;
            this.txtDescription.Text = resources.GetString("txtDescription.Text");
            // 
            // imgLogo
            // 
            this.imgLogo.Location = new System.Drawing.Point(8, 3);
            this.imgLogo.Name = "imgLogo";
            this.imgLogo.Size = new System.Drawing.Size(107, 33);
            this.imgLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgLogo.TabIndex = 0;
            this.imgLogo.TabStop = false;
            // 
            // imgBackground
            // 
            this.imgBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgBackground.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.imgBackground.Location = new System.Drawing.Point(0, 0);
            this.imgBackground.Name = "imgBackground";
            this.imgBackground.Size = new System.Drawing.Size(560, 260);
            this.imgBackground.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgBackground.TabIndex = 9;
            this.imgBackground.TabStop = false;
            // 
            // ProductLayout6
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlLayout);
            this.Name = "ProductLayout6";
            this.Size = new System.Drawing.Size(560, 260);
            this.Load += new System.EventHandler(this.ProductLayout6_Load);
            this.pnlLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgBackground)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLayout;
        private PictureBoxEx imgBackground;
        private RichTextBoxEx txtDescription;
        private PictureBoxEx imgLogo;
        private RichTextBoxEx txtDisclaimer;
        
    }
}
