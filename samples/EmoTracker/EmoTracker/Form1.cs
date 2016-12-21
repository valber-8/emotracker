using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpLibrary;

namespace EmoTracker
{
    public partial class Form1 : Form
    {
        bool Running = false;
        Emotracker.EmotionsTracker etr;
        public Form1()
        {
            InitializeComponent();
            // делаем невидимой нашу иконку в трее
            notifyIcon1.Visible = false;
            // добавляем Эвент или событие по 2му клику мышки, 
            //вызывая функцию  notifyIcon1_MouseDoubleClick
            //this.notifyIcon1.MouseDoubleClick += new MouseEventHandler(notifyIcon1_MouseDoubleClick);

            // добавляем событие на изменение окна
           // this.Resize += new System.EventHandler(this.Form1_Resize);

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // проверяем наше окно, и если оно было свернуто, делаем событие        
            if (WindowState == FormWindowState.Minimized)
            {
                // прячем наше окно из панели
                this.ShowInTaskbar = false;
                // делаем нашу иконку в трее активной
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // делаем нашу иконку скрытой
            notifyIcon1.Visible = false;
            // возвращаем отображение окна в панели
            this.ShowInTaskbar = true;
            //разворачиваем окно
            WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            // делаем нашу иконку скрытой
            notifyIcon1.Visible = false;
            // возвращаем отображение окна в панели
            this.ShowInTaskbar = true;
            //разворачиваем окно
            WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            // делаем нашу иконку скрытой
            notifyIcon1.Visible = false;
            // возвращаем отображение окна в панели
            this.ShowInTaskbar = true;
            //разворачиваем окно
            WindowState = FormWindowState.Normal;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Running)
            {

                etr = new Emotracker.EmotionsTracker();
                Emotracker.EmotionsConfiguration conf = etr.QueryConfiguration();
                if (textBox1.Text!="") conf.setCalibrationFilename(textBox1.Text);
                else conf.setCalibrationFilename("calib.bin");
                if (textBox3.Text != "") conf.setEmotionsFilename(textBox3.Text);
                else conf.setEmotionsFilename("1.ttml");
                if (textBox2.Text != "") conf.setStreamFilename(textBox2.Text);
                else conf.setStreamFilename("1.rssdk");
                //etr.Init();
                pxcmStatus status = etr.Start();
                toolStripStatusLabel1.Text = status.ToString();

                button1.Text = "Stop";
                stopToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = false;
                Running = true;
            }
            else
            {
                pxcmStatus status = etr.Stop();
                toolStripStatusLabel1.Text = status.ToString();
                //etr.Release();

                button1.Text = "Start";
                stopToolStripMenuItem.Enabled = false;
                startToolStripMenuItem.Enabled = true;
                Running = false;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "calibration files (*.bin)|*.bin|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = openFileDialog1.FileName;
            else
                toolStripStatusLabel1.Text = "Open calibration file failed";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "rssdk files (*.rssdk)|*.rssdk|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                textBox2.Text = saveFileDialog1.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog2 = new SaveFileDialog();
            saveFileDialog2.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
                textBox3.Text = saveFileDialog2.FileName;
        }
    }
}
