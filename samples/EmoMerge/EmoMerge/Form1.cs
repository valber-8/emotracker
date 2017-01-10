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

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox2.Text = openFileDialog1.FileName;
            else
                toolStripStatusLabel1.Text = "Open second file failed";

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
                             where DateTime.Parse(e.Attribute("end").Value) >= DateTime.Parse(c.Attribute("begin").Value) 
                                   && DateTime.Parse(e.Attribute("end").Value) <= DateTime.Parse(c.Attribute("end").Value)
                                   && DateTime.Parse(e.Attribute("begin").Value) <= DateTime.Parse(c.Attribute("begin").Value)
                             select e;
                var nested2 = from e in secondXml.Descendants(ns + "p")
                             where DateTime.Parse(e.Attribute("begin").Value) >= DateTime.Parse(c.Attribute("begin").Value)
                                   && DateTime.Parse(e.Attribute("begin").Value) <= DateTime.Parse(c.Attribute("end").Value)
                                   && DateTime.Parse(e.Attribute("end").Value) <= DateTime.Parse(c.Attribute("end").Value)
                             select e;
                var nested3 = from e in secondXml.Descendants(ns + "p")
                             where DateTime.Parse(e.Attribute("begin").Value) >= DateTime.Parse(c.Attribute("begin").Value)
                                   && DateTime.Parse(e.Attribute("begin").Value) <= DateTime.Parse(c.Attribute("end").Value)
                                   && DateTime.Parse(e.Attribute("end").Value) >= DateTime.Parse(c.Attribute("begin").Value)
                                   && DateTime.Parse(e.Attribute("end").Value) <= DateTime.Parse(c.Attribute("end").Value)
                             select e;
                var nested4 = from e in secondXml.Descendants(ns + "p")
                              where DateTime.Parse(e.Attribute("begin").Value) <= DateTime.Parse(c.Attribute("begin").Value)
                                    && DateTime.Parse(e.Attribute("begin").Value) >= DateTime.Parse(c.Attribute("end").Value)
                                    && DateTime.Parse(e.Attribute("end").Value) <= DateTime.Parse(c.Attribute("begin").Value)
                                    && DateTime.Parse(e.Attribute("end").Value) >= DateTime.Parse(c.Attribute("end").Value)
                              select e;

              
                foreach (XElement e in nested1) {
                    var node = c.Descendants(ns+"data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                    var cont = node.Parent.Value.Trim();
                    //var xdoc = XDocument.Parse(cont);
                    var xdoc = cont;

                    node = e.Descendants(ns+"data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                    cont = node.Parent.Value.Trim();
                    //var xdoc2 = XDocument.Parse(cont);
                    var xdoc2 = cont;

                    //xdoc.Add(xdoc2.Nodes());

                    content.Add(new XElement(ns + "p", new XAttribute("begin", e.Attribute("begin").Value), new XAttribute("end", c.Attribute("end").Value),
                                    new XElement("data", new XAttribute("type","text/plain; charset = us-ascii"),
                                        new XElement("metadata", new XAttribute("id","2")),               //change it
                                        new XCData(xdoc.ToString()+ xdoc2.ToString()),
                                        c.Descendants("span"),
                                        e.Descendants("span"),
                                        c.Value,
                                        e.Value
                                    )
                                ));         


                }


            }

            outXml.Save(output);

        }
    
        private void button3_Click(object sender, EventArgs e)
        {
            String outputFilename;
            if (!File.Exists(textBox1.Text)) {
                toolStripStatusLabel1.Text = "The first file is not exist";
                return;
            }
            if (!File.Exists(textBox2.Text))
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

            FileStream first = File.OpenRead(textBox1.Text);
            FileStream second = File.OpenRead(textBox2.Text);
            FileStream output = File.OpenWrite(outputFilename);
            toolStripStatusLabel1.Text = "Process...";
            Merge(first, second, output);
            toolStripStatusLabel1.Text = "Finished";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ttml files (*.ttml)|*.ttml|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = openFileDialog1.FileName;
            else
                toolStripStatusLabel1.Text = "Open first file failed";
        }
    }
}
