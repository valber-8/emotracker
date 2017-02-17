using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ttml2srt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2) {
                Console.Write("------------------------------------------------------------------------------\nThis program convert subtitles from ttml file with emotions records\nto srt subtitle file format\nUsage:\nttml2srt inputttmlfile outputsrtfile\n------------------------------------------------------------------------------\n");
                return;
            }

            FileStream input = File.OpenRead(args[0]);
            if (input == null) {
                Console.Write("Can not open file " + args[0] + " for reading");
                return;
            }
            FileStream output = File.Open(args[1], FileMode.Create, FileAccess.Write);
            if (output == null) {
                Console.Write("Can not open file " + args[1] + " for writing");
                return;
            }
            StreamWriter writer = new StreamWriter(output);
            //StringBuilder sb = new StringBuilder();

            int mid = 0;
            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            var inputXml = XDocument.Load(input);
            var query = from c in inputXml.Descendants(ns + "p")
                        select c;
            foreach (XElement p in query) {
                string emo = p.Nodes().OfType<XText>().First().Value.Trim();
                if (emo != "")
                {
                    writer.WriteLine("" + mid++);
                    writer.WriteLine(DateTime.Parse(p.Attribute("begin").Value).ToString("HH:mm:ss,fff") + " --> " + DateTime.Parse(p.Attribute("end").Value).ToString("HH:mm:ss,fff"));
                    writer.WriteLine(emo);
                    writer.WriteLine();

                }
            }
            writer.Dispose();
            output.Close();
            input.Close();


        }
    }
}
