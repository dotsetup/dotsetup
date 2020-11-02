namespace DotSetup
{
    partial class ProductLayout3
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
            this.txtTitle = new DotSetup.RichTextBoxEx();
            this.imgTitle = new DotSetup.PictureBoxEx();
            this.txtDescription = new DotSetup.RichTextBoxEx();
            this.txtDisclaimer = new DotSetup.RichTextBoxEx();
            this.imgBackground = new DotSetup.PictureBoxEx();
            this.pnlLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgTitle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgBackground)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlLayout
            // 
            this.pnlLayout.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayout.Controls.Add(this.txtTitle);
            this.pnlLayout.Controls.Add(this.imgTitle);
            this.pnlLayout.Controls.Add(this.txtDescription);
            this.pnlLayout.Controls.Add(this.txtDisclaimer);
            this.pnlLayout.Controls.Add(this.imgBackground);
            this.pnlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLayout.Location = new System.Drawing.Point(0, 0);
            this.pnlLayout.Name = "pnlLayout";
            this.pnlLayout.Size = new System.Drawing.Size(713, 379);
            this.pnlLayout.TabIndex = 11;
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTitle.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTitle.Location = new System.Drawing.Point(3, 45);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(376, 72);
            this.txtTitle.TabIndex = 13;
            this.txtTitle.Text = "";
            // 
            // imgTitle
            // 
            this.imgTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgTitle.Location = new System.Drawing.Point(3, 45);
            this.imgTitle.Name = "imgTitle";
            this.imgTitle.Size = new System.Drawing.Size(321, 43);
            this.imgTitle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgTitle.TabIndex = 14;
            this.imgTitle.TabStop = false;
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(3, 81);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(588, 252);
            this.txtDescription.TabIndex = 12;
            this.txtDescription.Text = "";
            // 
            // txtDisclaimer
            // 
            this.txtDisclaimer.BackColor = System.Drawing.Color.White;
            this.txtDisclaimer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDisclaimer.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtDisclaimer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtDisclaimer.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.txtDisclaimer.ForeColor = System.Drawing.SystemColors.InfoText;
            this.txtDisclaimer.Location = new System.Drawing.Point(0, 342);
            this.txtDisclaimer.Name = "txtDisclaimer";
            this.txtDisclaimer.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtDisclaimer.Size = new System.Drawing.Size(713, 37);
            this.txtDisclaimer.TabIndex = 11;
            this.txtDisclaimer.Text = "";
            // 
            // imgBackground
            // 
            this.imgBackground.Dock = System.Windows.Forms.DockStyle.Top;
            this.imgBackground.Location = new System.Drawing.Point(0, 0);
            this.imgBackground.Name = "imgBackground";
            this.imgBackground.Size = new System.Drawing.Size(713, 333);
            this.imgBackground.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgBackground.TabIndex = 9;
            this.imgBackground.TabStop = false;
            // 
            // ProductLayout3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlLayout);
            this.Name = "ProductLayout3";
            this.Size = new System.Drawing.Size(713, 379);
            this.Load += new System.EventHandler(this.ProductLayout3_Load);
            this.pnlLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgTitle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgBackground)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLayout;
        private PictureBoxEx imgBackground;
        private DotSetup.RichTextBoxEx txtDisclaimer;
        private RichTextBoxEx txtDescription;
        private PictureBoxEx imgTitle;
        private RichTextBoxEx txtTitle;
    }
}
