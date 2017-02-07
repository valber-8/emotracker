using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;
using System.Windows.Forms;

namespace EmoMerge
{
    
    public partial class Form1 : Form
    {

        

        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        
    
        private void button3_Click(object sender, EventArgs e)
        {
            String outputFilename;
            if (!File.Exists(mergeFirstFileTextBox.Text)) {
                toolStripStatusLabel1.Text = "The first file is not exist";
                return;
            }
            if (!File.Exists(mergeSecondFileTextBox.Text))
            {
                toolStripStatusLabel1.Text = "The second file is not exist";
                return;
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                outputFilename = saveFileDialog1.FileName;
            else
            {
                toolStripStatusLabel1.Text = "Select output filename";
                return;
            }

            FileStream first = File.OpenRead(mergeFirstFileTextBox.Text);
            FileStream second = File.OpenRead(mergeSecondFileTextBox.Text);
            FileStream output = File.Open(outputFilename,FileMode.Create,FileAccess.Write);
            toolStripStatusLabel1.Text = "Process...";
            this.Refresh();
            //try
            //{
                MergeUtilities.Merge(first, second, output);
            /*}
            catch 
            {
               toolStripStatusLabel1.Text = "Something goes wrong";
            }*/

            toolStripStatusLabel1.Text = "Finished";
            first.Close();
            second.Close();
            output.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                mergeFirstFileTextBox.Text = openFileDialog1.FileName;
            else
                toolStripStatusLabel1.Text = "Open first file failed";
        }


        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                mergeSecondFileTextBox.Text = openFileDialog1.FileName;
            else
                toolStripStatusLabel1.Text = "Open second file failed";

        }

        private void button7_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                averageFilenameTextBox.Text = openFileDialog1.FileName;
            else
                toolStripStatusLabel1.Text = "Open file failed";
        }


       

        private void button6_Click(object sender, EventArgs e)
        {

            String outputFilename;
            if (!File.Exists(averageFilenameTextBox.Text))
            {
                toolStripStatusLabel1.Text = "File is not exist";
                return;
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                outputFilename = saveFileDialog1.FileName;
            else
            {
                toolStripStatusLabel1.Text = "Select output filename";
                return;
            }

            FileStream first = File.OpenRead(averageFilenameTextBox.Text);
            FileStream output = File.Open(outputFilename, FileMode.Create, FileAccess.Write);
            toolStripStatusLabel1.Text = "Process...";
            this.Refresh();
            //try
            //{
            MergeUtilities.Average(first, output);
            //}
            //catch 
            //{
            // toolStripStatusLabel1.Text = "Something goes wrong";
            //}
            toolStripStatusLabel1.Text = "Finished";
            first.Close();
            output.Close();
        }



        
        private void button5_Click(object sender, EventArgs e)

        {

            String outputFilename;
            if (!File.Exists(smoothFilenameTextBox.Text))
            {
                toolStripStatusLabel1.Text = "File is not exist";
                return;
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                outputFilename = saveFileDialog1.FileName;
            else
            {
                toolStripStatusLabel1.Text = "Select output filename";
                return;
            }

            FileStream first = File.OpenRead(smoothFilenameTextBox.Text);
            FileStream output = File.Open(outputFilename, FileMode.Create, FileAccess.Write);
            toolStripStatusLabel1.Text = "Process...";
            this.Refresh();
            //try
            //{
            int interval = Convert.ToInt32(smoothIntervalTextBox.Text);
            MergeUtilities.Smooth(first, output, interval);
            //}
            //catch 
            //{
            // toolStripStatusLabel1.Text = "Something goes wrong";
            //}
            toolStripStatusLabel1.Text = "Finished";
            first.Close();
            output.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                smoothFilenameTextBox.Text = openFileDialog1.FileName;
            else
                toolStripStatusLabel1.Text = "Open file failed";

        }
    }



}
