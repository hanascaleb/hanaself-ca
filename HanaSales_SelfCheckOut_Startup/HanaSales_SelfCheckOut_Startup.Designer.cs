namespace HanaSales_SelfCheckOut_Startup
{
    partial class HanaSales_SelfCheckOut_Startup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HanaSales_SelfCheckOut_Startup));
            this.pbHMartLogo = new System.Windows.Forms.PictureBox();
            this.pn_Start = new System.Windows.Forms.Panel();
            this.lbStartupMessage = new System.Windows.Forms.Label();
            this.cPStartupWaiting = new DevComponents.DotNetBar.Controls.CircularProgress();
            this.UpdateReadyTimer = new System.Windows.Forms.Timer(this.components);
            this.bgUpdateWork = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pbHMartLogo)).BeginInit();
            this.pn_Start.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbHMartLogo
            // 
            this.pbHMartLogo.Image = ((System.Drawing.Image)(resources.GetObject("pbHMartLogo.Image")));
            this.pbHMartLogo.InitialImage = ((System.Drawing.Image)(resources.GetObject("pbHMartLogo.InitialImage")));
            this.pbHMartLogo.Location = new System.Drawing.Point(0, 0);
            this.pbHMartLogo.Margin = new System.Windows.Forms.Padding(0);
            this.pbHMartLogo.Name = "pbHMartLogo";
            this.pbHMartLogo.Size = new System.Drawing.Size(1024, 107);
            this.pbHMartLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbHMartLogo.TabIndex = 4;
            this.pbHMartLogo.TabStop = false;
            // 
            // pn_Start
            // 
            this.pn_Start.BackColor = System.Drawing.Color.White;
            this.pn_Start.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pn_Start.BackgroundImage")));
            this.pn_Start.Controls.Add(this.lbStartupMessage);
            this.pn_Start.Location = new System.Drawing.Point(0, 107);
            this.pn_Start.Margin = new System.Windows.Forms.Padding(0);
            this.pn_Start.Name = "pn_Start";
            this.pn_Start.Size = new System.Drawing.Size(1024, 660);
            this.pn_Start.TabIndex = 55;
            // 
            // lbStartupMessage
            // 
            this.lbStartupMessage.BackColor = System.Drawing.Color.Transparent;
            this.lbStartupMessage.Font = new System.Drawing.Font("Franklin Gothic Heavy", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStartupMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(46)))), ((int)(((byte)(95)))));
            this.lbStartupMessage.Location = new System.Drawing.Point(463, 0);
            this.lbStartupMessage.Name = "lbStartupMessage";
            this.lbStartupMessage.Size = new System.Drawing.Size(558, 203);
            this.lbStartupMessage.TabIndex = 51;
            this.lbStartupMessage.Text = "Please Wait..";
            this.lbStartupMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cPStartupWaiting
            // 
            this.cPStartupWaiting.AnimationSpeed = 80;
            this.cPStartupWaiting.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.cPStartupWaiting.BackgroundStyle.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cPStartupWaiting.BackgroundStyle.BackgroundImage")));
            this.cPStartupWaiting.BackgroundStyle.BackgroundImagePosition = DevComponents.DotNetBar.eStyleBackgroundImage.Center;
            this.cPStartupWaiting.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.cPStartupWaiting.Location = new System.Drawing.Point(462, 552);
            this.cPStartupWaiting.Name = "cPStartupWaiting";
            this.cPStartupWaiting.ProgressBarType = DevComponents.DotNetBar.eCircularProgressType.Dot;
            this.cPStartupWaiting.Size = new System.Drawing.Size(559, 212);
            this.cPStartupWaiting.Style = DevComponents.DotNetBar.eDotNetBarStyle.OfficeXP;
            this.cPStartupWaiting.TabIndex = 53;
            this.cPStartupWaiting.TabStop = false;
            // 
            // UpdateReadyTimer
            // 
            this.UpdateReadyTimer.Tick += new System.EventHandler(this.UpdateReadyTimer_Tick);
            // 
            // bgUpdateWork
            // 
            this.bgUpdateWork.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgUpdateWork_DoWork);
            this.bgUpdateWork.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgUpdateWork_RunWorkerCompleted);
            // 
            // HanaSales_SelfCheckOut_Startup
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.cPStartupWaiting);
            this.Controls.Add(this.pn_Start);
            this.Controls.Add(this.pbHMartLogo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HanaSales_SelfCheckOut_Startup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.pbHMartLogo)).EndInit();
            this.pn_Start.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbHMartLogo;
        private System.Windows.Forms.Panel pn_Start;
        private System.Windows.Forms.Label lbStartupMessage;
        private DevComponents.DotNetBar.Controls.CircularProgress cPStartupWaiting;
        private System.Windows.Forms.Timer UpdateReadyTimer;
        private System.ComponentModel.BackgroundWorker bgUpdateWork;
    }
}

