using System;
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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace GazeHeatMap
{
    public partial class GazeHeatMap : Form
    {
        public struct coords {
            public double x, y;
        };
        private double[,] heatmap;
        private IEnumerable<coords> points=null;


        public GazeHeatMap()
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.TransparencyKey = Color.Transparent;

            Process();
        }


        protected override void OnPaintBackground(PaintEventArgs e) {
            /* Ignore */
            //var src = new Bitmap(this.BackgroundImage);
            //e.Graphics.DrawImage(src, new Rectangle(0, 0, this.Width, this.Height));
            //base.OnPaintBackground(e);
            
            //Graphics g = e.Graphics;


            Bitmap bitmap = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bitmap.MakeTransparent();
            Graphics g = Graphics.FromImage(bitmap);

            if (points != null)
            {
                foreach (coords  a in this.points)
                {
                    int x = (int)(a.x / 100 * this.Width);
                    int y = (int)(a.y / 100 * this.Height);

                    using (var ellipsePath = new GraphicsPath())
                    {

                        var bounds = new Rectangle(x, y, 50, 50);
                        ellipsePath.AddEllipse(bounds);
                        var brush = new PathGradientBrush(ellipsePath);
                        Color[] colors = {
                               Color.FromArgb(64, 0, 0, 255),
                               Color.FromArgb(140, 0, 255, 0),
                               Color.FromArgb(216, 255, 255, 0),
                               Color.FromArgb(255, 255, 0, 0)
                        };
                        float[] relativePositions = { 0f, 0.25f, 0.5f, 1.0f };
                        ColorBlend colorBlend = new ColorBlend();
                        colorBlend.Colors = colors;
                        colorBlend.Positions = relativePositions;
                        brush.InterpolationColors = colorBlend;

                        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

                        g.FillEllipse(brush, bounds);
                        
                    }

                }
            }

            
            //this.BackgroundImage = bitmap;
            g.Dispose();


            ColorMatrix colormatrix = new ColorMatrix();
            colormatrix.Matrix33 = 0.8f;// opacityvalue;
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, this.Width, this.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imgAttribute);

            //e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, this.Width, this.Height));
            //this.BackColor = Color.Transparent;
            this.TransparencyKey = Color.Black;
        }

        public void Process() {
            FileStream input = null;
            FileStream output = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                input = File.OpenRead(openFileDialog1.FileName);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                output = File.Open(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write);

            if (input != null)
            {
                this.heatmap = HeatMap(input, output);
                input.Seek(0,SeekOrigin.Begin);
                this.points = Points(input, output);
                input.Close();
            }
            if (output != null)
                output.Close();

            MessageBox.Show("Processing finished");

        }

        public Color HeatColorMap(double value, double min, double max)
        {
            double val = (value - min) / (max - min);
            return Color.FromArgb(255, Convert.ToByte((decimal)(255 * val)), Convert.ToByte((decimal)(255 * (1 - val))), 0);
            /*
            return new Color
            {
                A = 255,
                R = Convert.ToByte((decimal)(255 * val)),
                G = Convert.ToByte((decimal)(255 * (1 - val))),
                B = 0
            };
            */
        }

        private double[,] HeatMap(FileStream first, FileStream output)
        {
            int size = 500;
            double[,] map = new double[size, size];
            double sigma = 1;
            int K = 51;
            double[,] kernel = new double[K, K];
            double mean = K / 2;
            double sum = 0.0; // For accumulating the kernel values
            for (int x = 0; x < K; ++x)
                for (int y = 0; y < K; ++y)
                {
                    kernel[x, y] = 1.0;// Math.Exp(-0.5 * (Math.Pow((x - mean) / sigma, 2.0) + Math.Pow((y - mean) / sigma, 2.0))) / (2 * Math.PI * sigma * sigma);

                    // Accumulate the kernel values
                    sum += kernel[x, y];
                }

            // Normalize the kernel
            for (int x = 0; x < K; ++x)
                for (int y = 0; y < K; ++y)
                    kernel[x, y] /= sum;


            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            var firstXml = XDocument.Load(first);
            var query = from c in firstXml.Descendants(ns + "p")
                        select c;

            foreach (XElement c in query)
            {

                var node = c.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                var cont = node.Parent.Value.Trim();
                var xdoc = XDocument.Parse("<root>" + cont + "</root>");

                var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                     .Select(a => new { x = Convert.ToDouble(a[0]), y = Convert.ToDouble(a[1]) });

                double tms = (DateTime.Parse(c.Attribute("end").Value) - DateTime.Parse(c.Attribute("begin").Value)).TotalMilliseconds;

                foreach (var g in gaze)
                {
                    if (g.x > 0 && g.y > 0 && g.x < 100 && g.y < 100)
                    {
                        int xx = (int)(g.x / 100 * size);
                        int yy = (int)(g.y / 100 * size);
                        //if (xx >= 0 && yy >= 0 && xx < size && yy < size)
                        map[xx, yy] += 1.0;// tms;
                    }
                }

            }

            double[,] omap = new double[size, size];
            for (int i = K / 2; i < size - K / 2; ++i) // iterate through image
            {
                for (int j = K / 2; j < size - K / 2; ++j)
                {
                    sum = 0.0; // sum will be the sum of input data * coeff terms
                    for (int ii = -K / 2; ii <= K / 2; ++ii) // iterate over kernel
                    {
                        for (int jj = -K / 2; jj <= K / 2; ++jj)
                        {
                            double data = map[i + ii, j + jj];
                            double coeff = kernel[ii + K / 2, jj + K / 2];
                            sum += data * coeff;
                        }
                    }
                    omap[i, j] = sum; // scale sum of convolution products and store in output
                }
            }

            double min = omap[0, 0], max = omap[0, 0];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    max = omap[i, j] > max ? omap[i, j] : max;
                    min = omap[i, j] < min ? omap[i, j] : min;
                }
            

            int horizontal = 1366, vertical = 768;
            var image = new Bitmap(horizontal, vertical, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(image))
            {
                g.Clear(Color.Transparent);
                for (int i = 0; i < horizontal; i++)
                    for (int j = 0; j < vertical; j++) {
                        image.SetPixel(i, j, HeatColorMap(omap[(int)(i  * size / horizontal), (int)(j * size / vertical)], min, max));
                    }

                //image.MakeTransparent(HeatColorMap(min,min,max));
            }
            

            if (output != null)
                image.Save(output, ImageFormat.Gif);

            return omap;
        }
        private IEnumerable<coords> Points(FileStream first, FileStream output)
        {

            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            var firstXml = XDocument.Load(first);
            var query = from c in firstXml.Descendants(ns + "p")
                        select c;
            List<coords> list = new List<coords>();

            foreach (XElement c in query)
            {

                var node = c.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                var cont = node.Parent.Value.Trim();
                var xdoc = XDocument.Parse("<root>" + cont + "</root>");

                var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                     .Select(a => new { x = Convert.ToDouble(a[0]), y = Convert.ToDouble(a[1]) });

                double tms = (DateTime.Parse(c.Attribute("end").Value) - DateTime.Parse(c.Attribute("begin").Value)).TotalMilliseconds;

                foreach (var g in gaze)
                {
                    if (g.x > 0 && g.y > 0 && g.x < 100 && g.y < 100)
                        list.Add(new coords { x = g.x, y = g.y });

                }

            }

            

            return list;
        }
    }
}
