namespace kake
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lstUsers = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkCommercials = new System.Windows.Forms.CheckBox();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblSong = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkRemote = new System.Windows.Forms.CheckBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtIPs = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lstSchedule = new System.Windows.Forms.ListView();
            this.colAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDateTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimeSleep = new System.Windows.Forms.DateTimePicker();
            this.lstSleepAction = new System.Windows.Forms.ComboBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstUsers
            // 
            this.lstUsers.FormattingEnabled = true;
            this.lstUsers.Location = new System.Drawing.Point(77, 21);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(192, 21);
            this.lstUsers.Sorted = true;
            this.lstUsers.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkCommercials);
            this.groupBox1.Controls.Add(this.btnStartStop);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.lstUsers);
            this.groupBox1.Location = new System.Drawing.Point(14, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(279, 105);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // chkCommercials
            // 
            this.chkCommercials.AutoSize = true;
            this.chkCommercials.Checked = true;
            this.chkCommercials.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCommercials.Location = new System.Drawing.Point(77, 49);
            this.chkCommercials.Name = "chkCommercials";
            this.chkCommercials.Size = new System.Drawing.Size(111, 17);
            this.chkCommercials.TabIndex = 7;
            this.chkCommercials.Text = "Mute commercials";
            this.chkCommercials.UseVisualStyleBackColor = true;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Location = new System.Drawing.Point(77, 71);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(192, 23);
            this.btnStartStop.TabIndex = 2;
            this.btnStartStop.Text = "Start me!";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Spotify user";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblSong);
            this.groupBox2.Controls.Add(this.lblStatus);
            this.groupBox2.Location = new System.Drawing.Point(14, 127);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(279, 92);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Status";
            // 
            // lblSong
            // 
            this.lblSong.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSong.Location = new System.Drawing.Point(11, 46);
            this.lblSong.Name = "lblSong";
            this.lblSong.Size = new System.Drawing.Size(258, 39);
            this.lblSong.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblStatus.Location = new System.Drawing.Point(11, 22);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(79, 13);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "not started";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Kake for Spotify";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkRemote);
            this.groupBox3.Controls.Add(this.txtLog);
            this.groupBox3.Controls.Add(this.txtIPs);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.txtKey);
            this.groupBox3.Controls.Add(this.txtPort);
            this.groupBox3.Location = new System.Drawing.Point(14, 361);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(559, 125);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Remote control";
            // 
            // chkRemote
            // 
            this.chkRemote.AutoSize = true;
            this.chkRemote.Location = new System.Drawing.Point(10, 19);
            this.chkRemote.Name = "chkRemote";
            this.chkRemote.Size = new System.Drawing.Size(129, 17);
            this.chkRemote.TabIndex = 12;
            this.chkRemote.Text = "Enable remote control";
            this.chkRemote.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(147, 19);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(404, 99);
            this.txtLog.TabIndex = 11;
            // 
            // txtIPs
            // 
            this.txtIPs.Location = new System.Drawing.Point(49, 98);
            this.txtIPs.Name = "txtIPs";
            this.txtIPs.Size = new System.Drawing.Size(89, 20);
            this.txtIPs.TabIndex = 10;
            this.txtIPs.Text = "192.168.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 101);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Accept:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Key:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Port:";
            // 
            // txtKey
            // 
            this.txtKey.Location = new System.Drawing.Point(49, 72);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(89, 20);
            this.txtKey.TabIndex = 1;
            this.txtKey.Text = "spotify";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(49, 46);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(89, 20);
            this.txtPort.TabIndex = 0;
            this.txtPort.Text = "80";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lstSchedule);
            this.groupBox4.Controls.Add(this.button1);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.dateTimeSleep);
            this.groupBox4.Controls.Add(this.lstSleepAction);
            this.groupBox4.Location = new System.Drawing.Point(299, 16);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(274, 203);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Event timer";
            // 
            // lstSchedule
            // 
            this.lstSchedule.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colAction,
            this.colDateTime});
            this.lstSchedule.FullRowSelect = true;
            this.lstSchedule.Location = new System.Drawing.Point(9, 42);
            this.lstSchedule.MultiSelect = false;
            this.lstSchedule.Name = "lstSchedule";
            this.lstSchedule.Size = new System.Drawing.Size(256, 149);
            this.lstSchedule.TabIndex = 15;
            this.lstSchedule.UseCompatibleStateImageBehavior = false;
            this.lstSchedule.View = System.Windows.Forms.View.Details;
            this.lstSchedule.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstSchedule_MouseDoubleClick);
            // 
            // colAction
            // 
            this.colAction.Text = "Action";
            this.colAction.Width = 154;
            // 
            // colDateTime
            // 
            this.colDateTime.Text = "Date/time";
            this.colDateTime.Width = 98;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(226, 15);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(39, 21);
            this.button1.TabIndex = 14;
            this.button1.Text = "Add";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(152, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(16, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "at";
            // 
            // dateTimeSleep
            // 
            this.dateTimeSleep.CustomFormat = "HH:mm";
            this.dateTimeSleep.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimeSleep.Location = new System.Drawing.Point(171, 16);
            this.dateTimeSleep.Name = "dateTimeSleep";
            this.dateTimeSleep.Size = new System.Drawing.Size(51, 20);
            this.dateTimeSleep.TabIndex = 2;
            // 
            // lstSleepAction
            // 
            this.lstSleepAction.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.lstSleepAction.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.lstSleepAction.FormattingEnabled = true;
            this.lstSleepAction.Items.AddRange(new object[] {
            "Pause playback",
            "Resume playback",
            "Quit Spotify",
            "Shutdown",
            "Go to Sleep",
            "Wake up from Sleep"});
            this.lstSleepAction.Location = new System.Drawing.Point(9, 15);
            this.lstSleepAction.Name = "lstSleepAction";
            this.lstSleepAction.Size = new System.Drawing.Size(139, 21);
            this.lstSleepAction.TabIndex = 1;
            this.lstSleepAction.Text = "Pause playback";
            this.lstSleepAction.SelectedIndexChanged += new System.EventHandler(this.lstSleepAction_SelectedIndexChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtFilter);
            this.groupBox5.Location = new System.Drawing.Point(14, 225);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(559, 131);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Philter by the following rules:";
            // 
            // txtFilter
            // 
            this.txtFilter.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFilter.Location = new System.Drawing.Point(9, 17);
            this.txtFilter.Multiline = true;
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFilter.Size = new System.Drawing.Size(541, 105);
            this.txtFilter.TabIndex = 0;
            this.txtFilter.Text = "artist eq \"rick astley\"\r\n";
            this.txtFilter.WordWrap = false;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 498);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Kake for Spotify";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox lstUsers;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblSong;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TextBox txtIPs;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtKey;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.CheckBox chkCommercials;
        private System.Windows.Forms.CheckBox chkRemote;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox lstSleepAction;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimeSleep;
        private System.Windows.Forms.ListView lstSchedule;
        private System.Windows.Forms.ColumnHeader colAction;
        private System.Windows.Forms.ColumnHeader colDateTime;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txtFilter;

    }
}

