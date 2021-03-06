﻿namespace EmoTracker
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.calibFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.streamFilename = new System.Windows.Forms.TextBox();
            this.emoFlename = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.usePersonTracker = new System.Windows.Forms.CheckBox();
            this.addGaze = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.gazeCollect = new System.Windows.Forms.CheckBox();
            this.calibButton = new System.Windows.Forms.Button();
            this.syncWithVlc = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(84, 163);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 29);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "EmotionsTracker";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.toolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(99, 70);
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.button1_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Enabled = false;
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.button1_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(98, 22);
            this.toolStripMenuItem1.Text = "Exit";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 197);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(284, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // calibFilename
            // 
            this.calibFilename.Location = new System.Drawing.Point(98, 12);
            this.calibFilename.Name = "calibFilename";
            this.calibFilename.Size = new System.Drawing.Size(150, 20);
            this.calibFilename.TabIndex = 2;
            this.calibFilename.Text = "calib.bin";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Calibration file";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "RSSDK output";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Emotions output";
            // 
            // streamFilename
            // 
            this.streamFilename.Location = new System.Drawing.Point(98, 39);
            this.streamFilename.Name = "streamFilename";
            this.streamFilename.Size = new System.Drawing.Size(150, 20);
            this.streamFilename.TabIndex = 6;
            this.streamFilename.Text = "1.rssdk";
            // 
            // emoFlename
            // 
            this.emoFlename.Location = new System.Drawing.Point(98, 63);
            this.emoFlename.Name = "emoFlename";
            this.emoFlename.Size = new System.Drawing.Size(150, 20);
            this.emoFlename.TabIndex = 7;
            this.emoFlename.Text = "1.ttml";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(251, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(18, 19);
            this.button2.TabIndex = 8;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(251, 40);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(18, 19);
            this.button3.TabIndex = 9;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(251, 64);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(18, 19);
            this.button4.TabIndex = 10;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // usePersonTracker
            // 
            this.usePersonTracker.AutoSize = true;
            this.usePersonTracker.Location = new System.Drawing.Point(17, 143);
            this.usePersonTracker.Name = "usePersonTracker";
            this.usePersonTracker.Size = new System.Drawing.Size(142, 17);
            this.usePersonTracker.TabIndex = 11;
            this.usePersonTracker.Text = "Use PersonTracker data";
            this.usePersonTracker.UseVisualStyleBackColor = true;
            this.usePersonTracker.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // addGaze
            // 
            this.addGaze.AutoSize = true;
            this.addGaze.Location = new System.Drawing.Point(17, 122);
            this.addGaze.Name = "addGaze";
            this.addGaze.Size = new System.Drawing.Size(97, 17);
            this.addGaze.TabIndex = 12;
            this.addGaze.Text = "Add gaze point";
            this.addGaze.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // gazeCollect
            // 
            this.gazeCollect.AutoSize = true;
            this.gazeCollect.Checked = true;
            this.gazeCollect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.gazeCollect.Location = new System.Drawing.Point(17, 101);
            this.gazeCollect.Name = "gazeCollect";
            this.gazeCollect.Size = new System.Drawing.Size(84, 17);
            this.gazeCollect.TabIndex = 13;
            this.gazeCollect.Text = "Collect gaze";
            this.gazeCollect.UseVisualStyleBackColor = true;
            this.gazeCollect.CheckedChanged += new System.EventHandler(this.gazeCollect_CheckedChanged);
            // 
            // calibButton
            // 
            this.calibButton.Location = new System.Drawing.Point(142, 95);
            this.calibButton.Name = "calibButton";
            this.calibButton.Size = new System.Drawing.Size(75, 23);
            this.calibButton.TabIndex = 14;
            this.calibButton.Text = "Calibrate";
            this.calibButton.UseVisualStyleBackColor = true;
            this.calibButton.Click += new System.EventHandler(this.calibButton_Click);
            // 
            // syncWithVlc
            // 
            this.syncWithVlc.AutoSize = true;
            this.syncWithVlc.Location = new System.Drawing.Point(143, 122);
            this.syncWithVlc.Name = "syncWithVlc";
            this.syncWithVlc.Size = new System.Drawing.Size(129, 17);
            this.syncWithVlc.TabIndex = 15;
            this.syncWithVlc.Text = "Synchronize with VLC";
            this.syncWithVlc.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 219);
            this.Controls.Add(this.syncWithVlc);
            this.Controls.Add(this.calibButton);
            this.Controls.Add(this.gazeCollect);
            this.Controls.Add(this.addGaze);
            this.Controls.Add(this.usePersonTracker);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.emoFlename);
            this.Controls.Add(this.streamFilename);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.calibFilename);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "EmoTracker";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox calibFilename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox streamFilename;
        private System.Windows.Forms.TextBox emoFlename;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox usePersonTracker;
        private System.Windows.Forms.CheckBox addGaze;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox gazeCollect;
        private System.Windows.Forms.Button calibButton;
        private System.Windows.Forms.CheckBox syncWithVlc;
    }
}

