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

        private void Merge(FileStream first, FileStream second, FileStream output) {
            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            var firstXml = XDocument.Load(first);
            var query = from c in firstXml.Descendants(ns+"p")
                        select c;
            var secondXml = XDocument.Load(second);

            XElement content;

            var outXml = new XElement(ns + "tt", 
                new XAttribute(XNamespace.Xmlns + "tts","http://www.w3.org/ns/ttml#styling"),
                new XAttribute(XNamespace.Xmlns + "ttm", "http://www.w3.org/ns/ttml#metadata"),
                new XElement("head",
                    new XElement("metadata", XNamespace.Xmlns + "ttm", "http://www.w3.org/ns/ttml#metadata",
                        new XElement(ttm+"title", "Timed Text TTML Emotions merged")
                    )
                ),
                new XElement("body",
                    content=new XElement("div")
                )
            );

            foreach (XElement c in query) {
                var nested1 = from e in secondXml.Descendants(ns+"p")
                             where DateTime.Parse(e.Attribute("end").Value) > DateTime.Parse(c.Attribute("begin").Value) 
                                   && DateTime.Parse(e.Attribute("end").Value) <= DateTime.Parse(c.Attribute("end").Value)
                                   && DateTime.Parse(e.Attribute("begin").Value) <= DateTime.Parse(c.Attribute("begin").Value)
                             select e;
                var nested2 = from e in secondXml.Descendants(ns + "p")
                             where DateTime.Parse(e.Attribute("begin").Value) >= DateTime.Parse(c.Attribute("begin").Value)
                                   && DateTime.Parse(e.Attribute("begin").Value) < DateTime.Parse(c.Attribute("end").Value)
                                   && DateTime.Parse(e.Attribute("end").Value) <= DateTime.Parse(c.Attribute("end").Value)
                             select e;
                var nested3 = from e in secondXml.Descendants(ns + "p")
                             where DateTime.Parse(e.Attribute("begin").Value) >= DateTime.Parse(c.Attribute("begin").Value)
                                   && DateTime.Parse(e.Attribute("begin").Value) < DateTime.Parse(c.Attribute("end").Value)
                                   && DateTime.Parse(e.Attribute("end").Value) > DateTime.Parse(c.Attribute("begin").Value)
                                   && DateTime.Parse(e.Attribute("end").Value) <= DateTime.Parse(c.Attribute("end").Value)
                             select e;
                var nested4 = from e in secondXml.Descendants(ns + "p")
                              where DateTime.Parse(e.Attribute("begin").Value) <= DateTime.Parse(c.Attribute("begin").Value)
                                    && DateTime.Parse(e.Attribute("begin").Value) > DateTime.Parse(c.Attribute("end").Value)
                                    && DateTime.Parse(e.Attribute("end").Value) < DateTime.Parse(c.Attribute("begin").Value)
                                    && DateTime.Parse(e.Attribute("end").Value) >= DateTime.Parse(c.Attribute("end").Value)
                              select e;

              
                foreach (XElement e in nested1) {
                    var node = c.Descendants(ns+"data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                    var cont = node.Parent.Value.Trim();
                    //XDocument xdoc = new XDocument(new XElement("root", cont));
                    var xdoc = XDocument.Parse("<root>"+cont+"</root>");
                    //var xdoc = cont;

                    foreach (XElement ee in xdoc.Descendants("Face")) {
                        ee.Attribute("id").Value = "1_" + ee.Attribute("id").Value;
                    }
                    foreach (XElement ee in xdoc.Descendants("Person"))
                    {
                        ee.Attribute("id").Value = "1_" + ee.Attribute("id").Value;
                    }


                    node = e.Descendants(ns+"data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                    cont = node.Parent.Value.Trim();
                    //var xdoc2 = XDocument.Parse(cont);
                    //xdoc.Value += cont;
                    //XCData xdoc = new XCData(cont);

                    var xdoc2 = XDocument.Parse("<root>" + cont + "</root>");

                    foreach (XElement ee in xdoc2.Descendants("Face"))
                    {
                        ee.Attribute("id").Value = "2_" + ee.Attribute("id").Value;
                    }
                    foreach (XElement ee in xdoc2.Descendants("Person"))
                    {
                        ee.Attribute("id").Value = "2_" + ee.Attribute("id").Value;
                    }
                    xdoc.Descendants("root").FirstOrDefault().Add(xdoc2.Descendants("root").FirstOrDefault().Nodes());
                    string cdata = "";
                    foreach (XElement ee in xdoc.Descendants("root").Nodes())
                        cdata +=ee.ToString();

                    content.Add(new XElement(ns + "p", new XAttribute("begin", c.Attribute("begin").Value), new XAttribute("end", e.Attribute("end").Value),
                                    new XElement(ns + "data", new XAttribute("type","text/plain; charset = us-ascii"),
                                        new XElement(ns + "metadata", new XAttribute("id","2")),               //change it
                                        //new XCData(xdoc.Descendants("root").DescendantNodes().ToString())
                                        //new XCData(string.Join("\n",xdoc.Descendants("root").DescendantNodes().ToString()))
                                        new XCData(cdata)
                                    ),
                                    c.Descendants(ns + "span"),
                                    e.Descendants(ns + "span"),
                                    c.Nodes().OfType<XText>(),
                                    e.Nodes().OfType<XText>()                                   
                                ));         


                }


            }

            outXml.Save(output);

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
            try
            {
                Merge(first, second, output);
            }
            catch 
            {
                toolStripStatusLabel1.Text = "Something goes wrong";
            }
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


        private void Average(FileStream first, FileStream output)
        {
            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            var firstXml = XDocument.Load(first);
            var query = from c in firstXml.Descendants(ns + "p")
                        select c;

            XElement content;

            var outXml = new XElement(ns + "tt",
                new XAttribute(XNamespace.Xmlns + "tts", "http://www.w3.org/ns/ttml#styling"),
                new XAttribute(XNamespace.Xmlns + "ttm", "http://www.w3.org/ns/ttml#metadata"),
                new XElement("head",
                    new XElement("metadata", XNamespace.Xmlns + "ttm", "http://www.w3.org/ns/ttml#metadata",
                        new XElement(ttm + "title", "Timed Text TTML Emotions merged")
                    )
                ),
                new XElement("body",
                    content = new XElement("div")
                )
            );
            int mid = 0;
            foreach (XElement c in query)
            {

                var node = c.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                var cont = node.Parent.Value.Trim();
                var xdoc = XDocument.Parse("<root>" + cont + "</root>");
                
                /*
                Dictionary<string, int> hash1 = new Dictionary<string, int>();
                Dictionary<string, double> hash2 = new Dictionary<string, double>();
                Dictionary<string, double> hash3 = new Dictionary<string, double>();
                foreach (XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))) {
                    if (hash1.ContainsKey(e.Name.ToString()))
                    {
                        hash1[e.Name.ToString()]++;
                        hash2[e.Name.ToString()] += Convert.ToDouble(e.Value);
                    }
                    else
                    {
                        hash1.Add(e.Name.ToString(), 1);
                        hash2.Add(e.Name.ToString(), Convert.ToDouble(e.Value));
                    }

                }
                
                XDocument cd = new XDocument(new XElement("root",
                    hash1.Select(kv => new XElement(kv.Key, hash2[kv.Key]/kv.Value)))
                    );
                    */

                IEnumerable<XElement> ll = from XElement e in xdoc.Descendants("Face").Nodes()
                    .Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value))
                    .Select(g => new XElement(g.Key, g.Average().ToString()))
                    select e;

                var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                     .Select(a => new { x = Convert.ToDouble(a[0]), y = Convert.ToDouble(a[1]) });
                XElement gg = new XElement("Gaze", gaze.Average(g => g.x).ToString() + " " + gaze.Average(g => g.y).ToString());
                
                var cd = new XDocument(new XElement("root",new XElement("Face",new XAttribute("id","0"), ll)));
                cd.Descendants("Face").FirstOrDefault().Add(gg);
                IEnumerable<XElement> pp = from XElement e in xdoc.Descendants("Person").Nodes()
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value))
                    .Select(g => new XElement(g.Key, g.Average().ToString()))
                    select e;
                if (pp.Count()>0)
                    cd.Element("root").Add(new XElement("Person", new XAttribute("id", "0"), pp));

                string cdata = "";
                foreach (XElement ee in cd.Descendants("root").Nodes())
                {
                    cdata += ee.ToString();
                }
                content.Add(new XElement(ns + "p", new XAttribute("begin", c.Attribute("begin").Value), new XAttribute("end", c.Attribute("end").Value),
                                    new XElement(ns + "data", new XAttribute("type", "text/plain; charset = us-ascii"),
                                        new XElement(ns + "metadata", new XAttribute("id", (mid++).ToString())), 
                                        new XCData(cdata)
                                    )/*,
                                    c.Descendants(ns + "span"),
                                    e.Descendants(ns + "span"),
                                    c.Nodes().OfType<XText>(),
                                    e.Nodes().OfType<XText>()*/
                                ));


                


            }

            outXml.Save(output);

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
            //try
            //{
            Average(first, output);
            //}
            //catch 
            //{
            // toolStripStatusLabel1.Text = "Something goes wrong";
            //}
            toolStripStatusLabel1.Text = "Finished";
            first.Close();
            output.Close();
        }



        private void Smooth(FileStream first, FileStream output, int interval)
        {
            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            var firstXml = XDocument.Load(first);
            var query = from c in firstXml.Descendants(ns + "p")
                        select c;

            XElement content;

            var outXml = new XElement(ns + "tt",
                new XAttribute(XNamespace.Xmlns + "tts", "http://www.w3.org/ns/ttml#styling"),
                new XAttribute(XNamespace.Xmlns + "ttm", "http://www.w3.org/ns/ttml#metadata"),
                new XElement("head",
                    new XElement("metadata", XNamespace.Xmlns + "ttm", "http://www.w3.org/ns/ttml#metadata",
                        new XElement(ttm + "title", "Timed Text TTML Emotions merged")
                    )
                ),
                new XElement("body",
                    content = new XElement("div")
                )
            );
            int mid = 0;
            foreach (XElement c in query)
            {

                var node = c.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                var cont = node.Parent.Value.Trim();
                var xdoc = XDocument.Parse("<root>" + cont + "</root>");

                /*
                Dictionary<string, int> hash1 = new Dictionary<string, int>();
                Dictionary<string, double> hash2 = new Dictionary<string, double>();
                Dictionary<string, double> hash3 = new Dictionary<string, double>();
                foreach (XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))) {
                    if (hash1.ContainsKey(e.Name.ToString()))
                    {
                        hash1[e.Name.ToString()]++;
                        hash2[e.Name.ToString()] += Convert.ToDouble(e.Value);
                    }
                    else
                    {
                        hash1.Add(e.Name.ToString(), 1);
                        hash2.Add(e.Name.ToString(), Convert.ToDouble(e.Value));
                    }

                }

                XDocument cd = new XDocument(new XElement("root",
                    hash1.Select(kv => new XElement(kv.Key, hash2[kv.Key]/kv.Value)))
                    );
                    */

                IEnumerable<XElement> ll = from XElement e in xdoc.Descendants("Face").Nodes()
                    .Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value))
                    .Select(g => new XElement(g.Key, g.Average().ToString()))
                                           select e;

                var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                     .Select(a => new { x = Convert.ToDouble(a[0]), y = Convert.ToDouble(a[1]) });
                XElement gg = new XElement("Gaze", gaze.Average(g => g.x).ToString() + " " + gaze.Average(g => g.y).ToString());

                var cd = new XDocument(new XElement("root", new XElement("Face", new XAttribute("id", "0"), ll)));
                cd.Descendants("Face").FirstOrDefault().Add(gg);
                IEnumerable<XElement> pp = from XElement e in xdoc.Descendants("Person").Nodes()
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value))
                    .Select(g => new XElement(g.Key, g.Average().ToString()))
                                           select e;
                if (pp.Count() > 0)
                    cd.Element("root").Add(new XElement("Person", new XAttribute("id", "0"), pp));

                string cdata = "";
                foreach (XElement ee in cd.Descendants("root").Nodes())
                {
                    cdata += ee.ToString();
                }
                content.Add(new XElement(ns + "p", new XAttribute("begin", c.Attribute("begin").Value), new XAttribute("end", c.Attribute("end").Value),
                                    new XElement(ns + "data", new XAttribute("type", "text/plain; charset = us-ascii"),
                                        new XElement(ns + "metadata", new XAttribute("id", (mid++).ToString())),
                                        new XCData(cdata)
                                    )/*,
                                    c.Descendants(ns + "span"),
                                    e.Descendants(ns + "span"),
                                    c.Nodes().OfType<XText>(),
                                    e.Nodes().OfType<XText>()*/
                                ));





            }

            outXml.Save(output);

        }

        private void button5_Click(object sender, EventArgs e)

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
            //try
            //{
            int interval = Convert.ToInt32(smoothIntervalTextBox.Text);
            Smooth(first, output, interval);
            //}
            //catch 
            //{
            // toolStripStatusLabel1.Text = "Something goes wrong";
            //}
            toolStripStatusLabel1.Text = "Finished";
            first.Close();
            output.Close();
        }
    }



}
