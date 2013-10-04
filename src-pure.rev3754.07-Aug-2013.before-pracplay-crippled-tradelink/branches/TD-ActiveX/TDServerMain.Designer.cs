﻿namespace TDServer
{
    partial class TDServerMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TDServerMain));
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._login = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this._report = new System.Windows.Forms.Button();
            this._togmsg = new System.Windows.Forms.Button();
            this.chkboxProdEnabled = new System.Windows.Forms.CheckBox();
            this._pass = new System.Windows.Forms.TextBox();
            this._user = new System.Windows.Forms.TextBox();
            this.txtDebug = new System.Windows.Forms.TextBox();
            this.api = new Axtdaactx.AxTDAAPIComm();
            ((System.ComponentModel.ISupportInitialize)(this.api)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(110, 8);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Username:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(253, 12);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Password:";
            // 
            // _login
            // 
            this._login.Location = new System.Drawing.Point(393, 8);
            this._login.Margin = new System.Windows.Forms.Padding(2);
            this._login.Name = "_login";
            this._login.Size = new System.Drawing.Size(50, 28);
            this._login.TabIndex = 6;
            this._login.Text = "login";
            this.toolTip1.SetToolTip(this._login, "Email tradelink-users@googlegroups.com with any problems using TD ameritrade with" +
                    " tradelink");
            this._login.UseVisualStyleBackColor = true;
            this._login.Click += new System.EventHandler(this._login_Click);
            // 
            // _report
            // 
            this._report.Image = ((System.Drawing.Image)(resources.GetObject("_report.Image")));
            this._report.Location = new System.Drawing.Point(497, 12);
            this._report.Margin = new System.Windows.Forms.Padding(2);
            this._report.Name = "_report";
            this._report.Size = new System.Drawing.Size(21, 18);
            this._report.TabIndex = 10;
            this.toolTip1.SetToolTip(this._report, "report a bug");
            this._report.UseVisualStyleBackColor = true;
            this._report.Click += new System.EventHandler(this._report_Click);
            // 
            // _togmsg
            // 
            this._togmsg.Location = new System.Drawing.Point(460, 12);
            this._togmsg.Margin = new System.Windows.Forms.Padding(2);
            this._togmsg.Name = "_togmsg";
            this._togmsg.Size = new System.Drawing.Size(23, 18);
            this._togmsg.TabIndex = 9;
            this._togmsg.Text = "!";
            this.toolTip1.SetToolTip(this._togmsg, "toggle connector messages");
            this._togmsg.UseVisualStyleBackColor = true;
            this._togmsg.Click += new System.EventHandler(this._togmsg_Click);
            // 
            // chkboxProdEnabled
            // 
            this.chkboxProdEnabled.AutoSize = true;
            this.chkboxProdEnabled.Location = new System.Drawing.Point(28, 12);
            this.chkboxProdEnabled.Name = "chkboxProdEnabled";
            this.chkboxProdEnabled.Size = new System.Drawing.Size(77, 17);
            this.chkboxProdEnabled.TabIndex = 11;
            this.chkboxProdEnabled.Text = "Production";
            this.chkboxProdEnabled.UseVisualStyleBackColor = true;
            // 
            // _pass
            // 
            this._pass.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::TDServer.Properties.Settings.Default, "password", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._pass.Location = new System.Drawing.Point(311, 8);
            this._pass.Margin = new System.Windows.Forms.Padding(2);
            this._pass.Name = "_pass";
            this._pass.PasswordChar = '*';
            this._pass.Size = new System.Drawing.Size(68, 20);
            this._pass.TabIndex = 2;
            this._pass.Text = global::TDServer.Properties.Settings.Default.password;
            this._pass.UseSystemPasswordChar = true;
            // 
            // _user
            // 
            this._user.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::TDServer.Properties.Settings.Default, "username", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this._user.Location = new System.Drawing.Point(172, 8);
            this._user.Margin = new System.Windows.Forms.Padding(2);
            this._user.Name = "_user";
            this._user.Size = new System.Drawing.Size(68, 20);
            this._user.TabIndex = 1;
            this._user.Text = global::TDServer.Properties.Settings.Default.username;
            // 
            // txtDebug
            // 
            this.txtDebug.Location = new System.Drawing.Point(28, 84);
            this.txtDebug.Multiline = true;
            this.txtDebug.Name = "txtDebug";
            this.txtDebug.Size = new System.Drawing.Size(554, 301);
            this.txtDebug.TabIndex = 12;
            // 
            // api
            // 
            this.api.Location = new System.Drawing.Point(28, 45);
            this.api.Name = "api";
            this.api.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("api.OcxState")));
            this.api.Size = new System.Drawing.Size(199, 23);
            this.api.TabIndex = 13;
            // 
            // TDServerMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 413);
            this.Controls.Add(this.api);
            this.Controls.Add(this.txtDebug);
            this.Controls.Add(this.chkboxProdEnabled);
            this.Controls.Add(this._report);
            this.Controls.Add(this._togmsg);
            this.Controls.Add(this._login);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._pass);
            this.Controls.Add(this._user);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "TDServerMain";
            this.Text = "TDServer BETA";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            ((System.ComponentModel.ISupportInitialize)(this.api)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _user;
        private System.Windows.Forms.TextBox _pass;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button _login;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button _report;
        private System.Windows.Forms.Button _togmsg;
        private System.Windows.Forms.CheckBox chkboxProdEnabled;
        private System.Windows.Forms.TextBox txtDebug;
      
        private Axtdaactx.AxTDAAPIComm api;
    }
}

