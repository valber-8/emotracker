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
using System.IO;
using System.Net.Sockets;

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
                if (calibFilename.Text!="") conf.setCalibrationFilename(calibFilename.Text);
                else conf.setCalibrationFilename("calib.bin");
                if (emoFlename.Text != "") conf.setEmotionsFilename(emoFlename.Text);
                else conf.setEmotionsFilename("1.ttml");
                conf.setStreamFilename(streamFilename.Text==""?null:streamFilename.Text);
                //if (streamFilename.Text != "") conf.setStreamFilename(streamFilename.Text);
                //else conf.setStreamFilename("1.rssdk");
                //etr.Init();
                conf.setPersonTracking(usePersonTracker.Checked);
                conf.setRecordingGaze(gazeCollect.Checked);
                conf.setAddGazePoint(addGaze.Checked);

                pxcmStatus status = etr.Start();
                toolStripStatusLabel1.Text = status.ToString();

                button1.Text = "Stop";
                stopToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = false;
                Running = true;
                timer1.Start();

                if (syncWithVlc.Checked) {
                    try
                    {
                        TcpClient tcpClient = new TcpClient("127.0.0.1", 4040);
                        NetworkStream networkStream = tcpClient.GetStream();
                        StreamWriter clientStreamWriter = new StreamWriter(networkStream);
                        clientStreamWriter.Write("pause\r\n");
                        clientStreamWriter.Flush();
                        networkStream.Close();
                        tcpClient.Close();
                    }
                    catch (SocketException ex) {
                        toolStripStatusLabel1.Text = "Can't open network connection";
                    }
                }
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

                try
                {
                    TcpClient tcpClient = new TcpClient("127.0.0.1", 4040);
                    NetworkStream networkStream = tcpClient.GetStream();
                    StreamWriter clientStreamWriter = new StreamWriter(networkStream);
                    clientStreamWriter.Write("pause\r\n");
                    clientStreamWriter.Flush();
                    networkStream.Close();
                    tcpClient.Close();
                }
                catch (SocketException ex)
                {
                    toolStripStatusLabel1.Text = "Can't open network connection";
                }


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
                calibFilename.Text = openFileDialog1.FileName;
            else
                toolStripStatusLabel1.Text = "Open calibration file failed";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "rssdk files (*.rssdk)|*.rssdk|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                streamFilename.Text = saveFileDialog1.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog2 = new SaveFileDialog();
            saveFileDialog2.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
                emoFlename.Text = saveFileDialog2.FileName;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = etr.getStatus().ToString();
            //timer1.Stop();
            if (etr.getStatus() < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                button1.Text = "Start";
                stopToolStripMenuItem.Enabled = false;
                startToolStripMenuItem.Enabled = true;
                Running = false;
            }
        }

        private void gazeCollect_CheckedChanged(object sender, EventArgs e)
        {
            if (gazeCollect.Checked)
            {
                addGaze.Enabled = true;
                calibFilename.Enabled = true;
            }
            else
            {
                addGaze.Enabled = false;
                calibFilename.Enabled = false;
            }
        }

        private void calibButton_Click(object sender, EventArgs e)
        {
            //var g=CreateGraphics();

            Form f = new Form();
            f.BackColor = Color.White;
            f.FormBorderStyle = FormBorderStyle.None;
            f.Bounds = Screen.PrimaryScreen.Bounds;
            //f.TopMost = true;
            f.Left = 0;
            f.Top = 0;
            f.Size = new Size(SystemInformation.VirtualScreen.Width,
                              SystemInformation.VirtualScreen.Height);
            f.Show();


            PXCMSession session = PXCMSession.CreateInstance();
            PXCMSenseManager sm = session.CreateSenseManager();// PXCMSenseManager.CreateInstance();
            sm.EnableFace();
            PXCMFaceModule face = sm.QueryFace();
            PXCMFaceConfiguration facec = face.CreateActiveConfiguration();
            PXCMFaceConfiguration.GazeConfiguration gazec = facec.QueryGaze();
            gazec.isEnabled = true;
            facec.ApplyChanges();
            // Initialize the pipeline
            sm.Init();
            //using (PXCMFaceData output = face.CreateOutput()) { 
            Boolean calibration=true;
                while (calibration&&sm.AcquireFrame(false).IsSuccessful())
                {
                    PXCMFaceModule face2 = sm.QueryFace();
                    if (face2 != null)
                    {
                    PXCMFaceData output=face2.CreateOutput();
                    output.Update();
                    if (output.QueryNumberOfDetectedFaces() > 0)
                    {
                        PXCMFaceData.GazeCalibData faced = output.QueryFaceByIndex(0).QueryGazeCalibration();
                        if (faced != null)
                        {
                            PXCMPointI32 calibp;
                            PXCMFaceData.GazeCalibData.CalibrationState state = faced.QueryCalibrationState();
                            switch (state)
                            {
                                case PXCMFaceData.GazeCalibData.CalibrationState.CALIBRATION_IDLE:
                                    // Visual cue to the user that the calibration process starts, or LoadCalibData.
                                    break;

                                case PXCMFaceData.GazeCalibData.CalibrationState.CALIBRATION_NEW_POINT:
                                    // Visual cue to the user that a new calibration point is available.
                                    {
                                        calibp = faced.QueryCalibPoint();

                                        System.Drawing.Graphics graphics = f.CreateGraphics();
                                        graphics.Clear(f.BackColor);
                                        System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(calibp.x - 50, calibp.y - 50, 100, 100);
                                        graphics.DrawRectangle(System.Drawing.Pens.Red, rectangle);
                                        graphics.FillRectangle(new SolidBrush(Color.Red), rectangle);
                                        // set the cursor to that point                            
                                        break;
                                    }
                                case PXCMFaceData.GazeCalibData.CalibrationState.CALIBRATION_SAME_POINT:
                                    // Continue visual cue to the user at the same location.
                                    {
                                        calibp = faced.QueryCalibPoint();
                                        System.Drawing.Graphics graphics = this.CreateGraphics();
                                        System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(
                                           calibp.x - 50, calibp.y - 50, 100, 100);
                                        graphics.DrawRectangle(System.Drawing.Pens.Red, rectangle);
                                        graphics.FillRectangle(new SolidBrush(Color.Red), rectangle);
                                        // set the cursor to that point                         
                                        break;
                                    }

                                case PXCMFaceData.GazeCalibData.CalibrationState.CALIBRATION_DONE:
                                    // Visual cue to the user that the calibration process is complete or calibration data is loaded.
                                    // Optionally save the calibration data.

                                    int calibBuffersize = faced.QueryCalibDataSize();
                                    byte[] calibBuffer = new byte[calibBuffersize];
                                    var calib_status = faced.QueryCalibData(out calibBuffer);

                                    SaveFileDialog saveFileDialog2 = new SaveFileDialog();
                                    saveFileDialog2.Filter = "calibration files (*.bin)|*.bin|All files (*.*)|*.*";
                                    if (saveFileDialog2.ShowDialog() == DialogResult.OK)
                                    {
                                        calibFilename.Text = saveFileDialog2.FileName;
                                        File.WriteAllBytes(calibFilename.Text, calibBuffer);
                                    }
                                    calibration = false;
                                    break;

                            }
                        }
                    }
                }
                // Resume next frame processing
                sm.ReleaseFrame();
            }
            





            facec.Dispose();
            sm.Dispose();
            session.Dispose();
            //f.WindowState = FormWindowState.Maximized;
            //Application.EnableVisualStyles();
            //Application.Run(f);
            f.Close();
            f.Dispose();
        }
    }
}
