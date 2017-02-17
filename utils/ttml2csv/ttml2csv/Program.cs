using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ttml2srt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Write("------------------------------------------------------------------------------\nThis program convert ttml file with emotions records\nto csv file format\nUsage:\nttml2csv inputttmlfile outputcsvfile\n------------------------------------------------------------------------------\n");
                return;
            }

            


            FileStream input = File.OpenRead(args[0]);
            if (input == null)
            {
                Console.Write("Can not open file " + args[0] + " for reading");
                return;
            }
            FileStream output = File.Open(args[1], FileMode.Create, FileAccess.Write);
            if (output == null)
            {
                Console.Write("Can not open file " + args[1] + " for writing");
                return;
            }
            StreamWriter writer = new StreamWriter(output);
            //StringBuilder sb = new StringBuilder();

                    var map = new string[] {"Brow_Raised_Left","Brow_Raised_Right","Brow_Lowered_Left",
                    "Brow_Lowered_Right","Smile","Kiss","Mouth_Open","Closed_Eye_Left","Closed_Eye_Right",
                    "Eyes_Turn_Left","Eyes_Turn_Right","Eyes_Up","Eyes_Down","Tongue_Out","Puff_Right_Cheek",
                    "Puff_Left_Cheek","Yaw","Pitch","Roll","HeadX","HeadY","HeadZ","Pulse"};
            int mid = 0;
            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            var inputXml = XDocument.Load(input);
            var query = from c in inputXml.Descendants(ns + "p")
                        select c;
            writer.Write("begin,end,");
            for (int i = 0; i < map.Length; i++)
                writer.Write(map[i] + ",");
            writer.Write("GazeX,GazeY");
            foreach (XElement p in query)
            {

                var node = p.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                var cont = node.Parent.Value.Trim();
                var xdoc = XDocument.Parse("<root>" + cont + "</root>");
                /*
                IEnumerable<XElement> ll = from XElement e in xdoc.Descendants("Face").Nodes()
                    .Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value))
                    .Select(g => new XElement(g.Key, g.Where(v => Math.Abs(v) < GlobalVar.MaxAccountedVal).Average().ToString()))
                                           select e;
                                           */

                writer.Write(DateTime.Parse(p.Attribute("begin").Value).ToString("HH:mm:ss.ff") + "," + DateTime.Parse(p.Attribute("end").Value).ToString("HH:mm:ss.ff")+",");
                    for (int i = 0; i < map.Length; i++)
                    {
                        var f = xdoc.Descendants("Face");
                        if (f.Count()>0) {
                            var v = f.Descendants(map[i]);
                            if (v.Count()>0) writer.Write(v.First().Value + ",");
                            else writer.Write(",");
                        }else writer.Write(",");
                    }        
                    var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                     .Select(a => new { x = Convert.ToDouble(a[0]), y = Convert.ToDouble(a[1]) });
                if (gaze.Count() > 0)
                    writer.WriteLine(gaze.First().x + "," + gaze.First().y);
                else writer.WriteLine(",");

            }
            writer.Dispose();
            output.Close();
            input.Close();


        }
    }
}
