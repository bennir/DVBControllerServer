namespace DVBViewerController
{
    partial class DVBServer
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DVBServer));
            this.btnDebug = new System.Windows.Forms.Button();
            this.runServer = new System.Windows.Forms.Button();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.stopServer = new System.Windows.Forms.Button();
            this.DVBNotification = new System.Windows.Forms.NotifyIcon(this.components);
            this.lblPort = new System.Windows.Forms.Label();
            this.cbStartServer = new System.Windows.Forms.CheckBox();
            this.cbMinimizeOnStart = new System.Windows.Forms.CheckBox();
            this.cbDebug = new System.Windows.Forms.CheckBox();
            this.btnRecService = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDebug
            // 
            this.btnDebug.Location = new System.Drawing.Point(262, 76);
            this.btnDebug.Name = "btnDebug";
            this.btnDebug.Size = new System.Drawing.Size(92, 23);
            this.btnDebug.TabIndex = 0;
            this.btnDebug.Text = "SendCommand";
            this.btnDebug.UseVisualStyleBackColor = true;
            this.btnDebug.Click += new System.EventHandler(this.btnDebug_Click);
            // 
            // runServer
            // 
            this.runServer.Location = new System.Drawing.Point(102, 8);
            this.runServer.Name = "runServer";
            this.runServer.Size = new System.Drawing.Size(75, 23);
            this.runServer.TabIndex = 2;
            this.runServer.Text = "Start";
            this.runServer.UseVisualStyleBackColor = true;
            this.runServer.Click += new System.EventHandler(this.runServer_Click);
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(48, 10);
            this.tbPort.MaxLength = 5;
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(48, 20);
            this.tbPort.TabIndex = 4;
            this.tbPort.Text = "8000";
            // 
            // tbLog
            // 
            this.tbLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLog.Location = new System.Drawing.Point(12, 103);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLog.Size = new System.Drawing.Size(342, 312);
            this.tbLog.TabIndex = 5;
            // 
            // stopServer
            // 
            this.stopServer.Location = new System.Drawing.Point(102, 8);
            this.stopServer.Name = "stopServer";
            this.stopServer.Size = new System.Drawing.Size(75, 23);
            this.stopServer.TabIndex = 6;
            this.stopServer.Text = "Stop";
            this.stopServer.UseVisualStyleBackColor = true;
            this.stopServer.Visible = false;
            this.stopServer.Click += new System.EventHandler(this.stopServer_Click);
            // 
            // DVBNotification
            // 
            this.DVBNotification.Icon = ((System.Drawing.Icon)(resources.GetObject("DVBNotification.Icon")));
            this.DVBNotification.Text = "DVBViewer Controller";
            this.DVBNotification.Click += new System.EventHandler(this.DVBNotification_Click);
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(13, 13);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.TabIndex = 7;
            this.lblPort.Text = "Port:";
            // 
            // cbStartServer
            // 
            this.cbStartServer.AutoSize = true;
            this.cbStartServer.Location = new System.Drawing.Point(16, 36);
            this.cbStartServer.Name = "cbStartServer";
            this.cbStartServer.Size = new System.Drawing.Size(146, 17);
            this.cbStartServer.TabIndex = 8;
            this.cbStartServer.Text = "Start Server automatically";
            this.cbStartServer.UseVisualStyleBackColor = true;
            this.cbStartServer.CheckedChanged += new System.EventHandler(this.cbStartServer_CheckedChanged);
            // 
            // cbMinimizeOnStart
            // 
            this.cbMinimizeOnStart.AutoSize = true;
            this.cbMinimizeOnStart.Location = new System.Drawing.Point(16, 59);
            this.cbMinimizeOnStart.Name = "cbMinimizeOnStart";
            this.cbMinimizeOnStart.Size = new System.Drawing.Size(104, 17);
            this.cbMinimizeOnStart.TabIndex = 9;
            this.cbMinimizeOnStart.Text = "Minimize on start";
            this.cbMinimizeOnStart.UseVisualStyleBackColor = true;
            this.cbMinimizeOnStart.CheckedChanged += new System.EventHandler(this.cbMinimizeOnStart_CheckedChanged);
            // 
            // cbDebug
            // 
            this.cbDebug.AutoSize = true;
            this.cbDebug.Location = new System.Drawing.Point(16, 80);
            this.cbDebug.Name = "cbDebug";
            this.cbDebug.Size = new System.Drawing.Size(88, 17);
            this.cbDebug.TabIndex = 10;
            this.cbDebug.Text = "Debug Mode";
            this.cbDebug.UseVisualStyleBackColor = true;
            this.cbDebug.CheckedChanged += new System.EventHandler(this.cbDebug_CheckedChanged);
            // 
            // btnRecService
            // 
            this.btnRecService.Location = new System.Drawing.Point(241, 8);
            this.btnRecService.Name = "btnRecService";
            this.btnRecService.Size = new System.Drawing.Size(113, 23);
            this.btnRecService.TabIndex = 11;
            this.btnRecService.Text = "Recording Service";
            this.btnRecService.UseVisualStyleBackColor = true;
            this.btnRecService.Click += new System.EventHandler(this.btnRecService_Click);
            // 
            // DVBServer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(359, 418);
            this.Controls.Add(this.btnRecService);
            this.Controls.Add(this.cbDebug);
            this.Controls.Add(this.cbMinimizeOnStart);
            this.Controls.Add(this.cbStartServer);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.stopServer);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.tbPort);
            this.Controls.Add(this.runServer);
            this.Controls.Add(this.btnDebug);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DVBServer";
            this.Text = "DVBViewer Controller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DVBServer_FormClosing);
            this.Load += new System.EventHandler(this.DVBServer_Load);
            this.Resize += new System.EventHandler(this.DVBServer_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDebug;
        private System.Windows.Forms.Button runServer;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button stopServer;
        private System.Windows.Forms.NotifyIcon DVBNotification;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.CheckBox cbStartServer;
        private System.Windows.Forms.CheckBox cbMinimizeOnStart;
        private System.Windows.Forms.CheckBox cbDebug;
        private System.Windows.Forms.Button btnRecService;
    }
}

