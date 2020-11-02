namespace DotSetup
{
    partial class FormTestSDK
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
            this.components = new System.ComponentModel.Container();
            this.ButtonSetWindow = new System.Windows.Forms.Button();
            this.ButtonAccept = new System.Windows.Forms.Button();
            this.ButtonDecline = new System.Windows.Forms.Button();
            this.ButtonShowHide = new System.Windows.Forms.Button();
            this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.ButtonInstall = new System.Windows.Forms.Button();
            this.TimerProgress = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // ButtonSetWindow
            // 
            this.ButtonSetWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonSetWindow.Location = new System.Drawing.Point(41, 439);
            this.ButtonSetWindow.Name = "ButtonSetWindow";
            this.ButtonSetWindow.Size = new System.Drawing.Size(75, 23);
            this.ButtonSetWindow.TabIndex = 0;
            this.ButtonSetWindow.Text = "SetWindow";
            this.ButtonSetWindow.UseVisualStyleBackColor = true;
            this.ButtonSetWindow.Click += new System.EventHandler(this.ButtonSetWindow_Click);
            // 
            // ButtonAccept
            // 
            this.ButtonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonAccept.Location = new System.Drawing.Point(284, 439);
            this.ButtonAccept.Name = "ButtonAccept";
            this.ButtonAccept.Size = new System.Drawing.Size(75, 23);
            this.ButtonAccept.TabIndex = 1;
            this.ButtonAccept.Text = "Accept";
            this.ButtonAccept.UseVisualStyleBackColor = true;
            this.ButtonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // ButtonDecline
            // 
            this.ButtonDecline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonDecline.Location = new System.Drawing.Point(203, 439);
            this.ButtonDecline.Name = "ButtonDecline";
            this.ButtonDecline.Size = new System.Drawing.Size(75, 23);
            this.ButtonDecline.TabIndex = 2;
            this.ButtonDecline.Text = "Decline";
            this.ButtonDecline.UseVisualStyleBackColor = true;
            this.ButtonDecline.Click += new System.EventHandler(this.ButtonDecline_Click);
            // 
            // ButtonShowHide
            // 
            this.ButtonShowHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonShowHide.Location = new System.Drawing.Point(122, 439);
            this.ButtonShowHide.Name = "ButtonShowHide";
            this.ButtonShowHide.Size = new System.Drawing.Size(75, 23);
            this.ButtonShowHide.TabIndex = 3;
            this.ButtonShowHide.Text = "Show";
            this.ButtonShowHide.UseVisualStyleBackColor = true;
            this.ButtonShowHide.Click += new System.EventHandler(this.ButtonShowHide_Click);
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBar1.Location = new System.Drawing.Point(446, 439);
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(240, 23);
            this.ProgressBar1.TabIndex = 4;
            // 
            // ButtonInstall
            // 
            this.ButtonInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonInstall.Location = new System.Drawing.Point(365, 439);
            this.ButtonInstall.Name = "ButtonInstall";
            this.ButtonInstall.Size = new System.Drawing.Size(75, 23);
            this.ButtonInstall.TabIndex = 5;
            this.ButtonInstall.Text = "Install";
            this.ButtonInstall.UseVisualStyleBackColor = true;
            this.ButtonInstall.Click += new System.EventHandler(this.ButtonInstall_Click);
            // 
            // TimerProgress
            // 
            this.TimerProgress.Tick += new System.EventHandler(this.TimerProgress_Tick);
            // 
            // FormTestSDK
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(718, 490);
            this.Controls.Add(this.ButtonInstall);
            this.Controls.Add(this.ProgressBar1);
            this.Controls.Add(this.ButtonShowHide);
            this.Controls.Add(this.ButtonDecline);
            this.Controls.Add(this.ButtonAccept);
            this.Controls.Add(this.ButtonSetWindow);
            this.Name = "FormTestSDK";
            this.Text = "TestSDK";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTestSDK_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonSetWindow;
        private System.Windows.Forms.Button ButtonAccept;
        private System.Windows.Forms.Button ButtonDecline;
        private System.Windows.Forms.Button ButtonShowHide;
        private System.Windows.Forms.ProgressBar ProgressBar1;
        private System.Windows.Forms.Button ButtonInstall;
        private System.Windows.Forms.Timer TimerProgress;
    }
}