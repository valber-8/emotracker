using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace EmoMerge
{
    public static class GlobalVar
    {
        public const double MaxAccountedVal = 2000.0;
    }

    public class MergeUtilities
    {
        public MergeUtilities()
        {
        }

        public static void Merge(Stream first, Stream second, Stream output)
        {

            int mid = 0;
            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";            

            var firstXml = XDocument.Load(first);
            var secondXml = XDocument.Load(second);

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

            var timeList = (from c in firstXml.Descendants(ns + "p")
                             select DateTime.Parse(c.Attribute("begin").Value))
                            .Union(from c in firstXml.Descendants(ns + "p")
                                   select DateTime.Parse(c.Attribute("end").Value))
                            .Union(from c in secondXml.Descendants(ns + "p")
                                   select DateTime.Parse(c.Attribute("begin").Value))
                            .Union(from c in secondXml.Descendants(ns + "p")
                                   select DateTime.Parse(c.Attribute("end").Value))
                                   .OrderBy(p => p);
            //arr.Skip(1).Zip(arr, (second, first) => new[] { first, second }).ToArr
            foreach (var d in timeList.Skip(1).Zip(timeList,(e,b) => new {b,e})) {
                var c1 = firstXml.Descendants(ns + "p").Where(el => DateTime.Parse(el.Attribute("begin").Value) <= d.b && DateTime.Parse(el.Attribute("end").Value) >= d.e);
                var c2 = secondXml.Descendants(ns + "p").Where(el => DateTime.Parse(el.Attribute("begin").Value) <= d.b && DateTime.Parse(el.Attribute("end").Value) >= d.e);
                XDocument xdoc = XDocument.Parse("<root></root>");

                List<XElement> nodes = Enumerable.Empty<XElement>().ToList();
                List<XText> text = Enumerable.Empty<XText>().ToList();
                if (c1.Any())
                {
                    var node = c1.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                    var cont = node.Parent.Value.Trim();
                    var xdoc2 = XDocument.Parse("<root>" + cont + "</root>");

                    foreach (XElement ee in xdoc2.Descendants("Face"))
                    {
                        ee.Attribute("id").Value = "1_" + ee.Attribute("id").Value;
                    }
                    foreach (XElement ee in xdoc2.Descendants("Person"))
                    {
                        ee.Attribute("id").Value = "1_" + ee.Attribute("id").Value;
                    }
                    xdoc.Descendants("root").FirstOrDefault().Add(xdoc2.Descendants("root").FirstOrDefault().Nodes());
                    nodes.Add(c1.Descendants(ns + "span").FirstOrDefault());
                    text.Add(c1.Nodes().OfType<XText>().FirstOrDefault());
                }
                if (c2.Any())
                {
                    var node = c2.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                    var cont = node.Parent.Value.Trim();

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
                    nodes.Add(c2.Descendants(ns + "span").FirstOrDefault());
                    text.Add(c2.Nodes().OfType<XText>().FirstOrDefault());
                }
                string cdata = "";
                foreach (XElement ee in xdoc.Descendants("root").Nodes())
                    cdata += ee.ToString();

                content.Add(new XElement(ns + "p", new XAttribute("begin", d.b.ToString("HH:mm:ss.ff", System.Globalization.CultureInfo.InvariantCulture)),
                                new XAttribute("end", d.e.ToString("HH:mm:ss.ff", System.Globalization.CultureInfo.InvariantCulture)),
                                new XElement(ns + "data", new XAttribute("type", "text/plain; charset = us-ascii"),
                                    new XElement(ns + "metadata", new XAttribute("id", (mid++).ToString())),               //change it
                                                                                                                           //new XCData(xdoc.Descendants("root").DescendantNodes().ToString())
                                                                                                                           //new XCData(string.Join("\n",xdoc.Descendants("root").DescendantNodes().ToString()))
                                    new XCData(cdata)
                                ),
                                nodes,text)
                            );

                

            }

            
            outXml.Save(output);

        }
        public static void Average(Stream first, Stream output)
        {
            int mid = 0;
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
            foreach (XElement c in query)
            {

                var node = c.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                var cont = node.Parent.Value.Trim();
                var xdoc = XDocument.Parse("<root>" + cont + "</root>");

                IEnumerable<XElement> ff = from XElement e in xdoc.Descendants("Face").Nodes()
                    .Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => (double?)Convert.ToDouble(((XElement)v).Value, CultureInfo.InvariantCulture))
                    .Select(g => new XElement(g.Key, g.Where(v => Math.Abs((double)v) < GlobalVar.MaxAccountedVal).Average().ToString()))
                                           select e;

                var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                     .Select(a => new { x = Convert.ToDouble(a[0], CultureInfo.InvariantCulture), y = Convert.ToDouble(a[1], CultureInfo.InvariantCulture) });
                var cd = new XDocument(new XElement("root"));
                if (ff.Any())  cd.Element("root").Add(new XElement("Face", new XAttribute("id", "0"), ff));

                string gazemark = null;
                if (gaze.Any())
                {
                    XElement gg = new XElement("Gaze", gaze.Average(g => g.x).ToString() + " " + gaze.Average(g => g.y).ToString());
                    cd.Descendants("Face").FirstOrDefault().Add(gg);
                    gazemark = gaze.Average(g => g.x).ToString() + "% " + gaze.Average(g => g.y).ToString() + "%";
                }

                IEnumerable<XElement> pp = from XElement e in xdoc.Descendants("Person").Nodes()
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value, CultureInfo.InvariantCulture))
                    .Select(g => new XElement(g.Key, g.Average().ToString()))
                                           select e;
                if (pp.Any())
                    cd.Element("root").Add(new XElement("Person", new XAttribute("id", "0"), pp));

                /*var emotion = (from XElement p in query
                               group p by p.Nodes().OfType<XText>().ToString() into gp
                               orderby gp.Count() descending
                               select gp)
                                    .Take(1)
                                    .Select(g => g.Key);

                var emotion = (from string e in c.Nodes().OfType<XText>().ToString().Split('\n') 
                               group e by e.Trim() into gp
                               orderby gp.Count() descending
                               select gp)
                                    .Take(1)
                                    .Select(g => g.Key).FirstOrDefault();
                                    */
                var emotion = (from string e in c.Nodes().OfType<XText>().FirstOrDefault().ToString().Split('\n')
                               group e by e.Trim() into gp
                               orderby gp.Count() descending
                               select gp.Key).Take(1).SingleOrDefault();

                string cdata = "";
                foreach (XElement ee in cd.Descendants("root").Nodes())
                {
                    cdata += ee.ToString();
                }



                content.Add(new XElement(ns + "p", new XAttribute("begin", c.Attribute("begin").Value), new XAttribute("end", c.Attribute("end").Value),
                                    new XElement(ns + "data", new XAttribute("type", "text/plain; charset = us-ascii"),
                                        new XElement(ns + "metadata", new XAttribute("id", (mid++).ToString())),
                                        new XCData(cdata)
                                    ),
                                    gazemark == null ? null : new XElement(ns + "span", new XAttribute(tts + "origin", gazemark), new XText("+")),
                                    emotion
                                    /*,
                                        c.Descendants(ns + "span"),
                                        e.Descendants(ns + "span"),
                                        c.Nodes().OfType<XText>(),
                                        e.Nodes().OfType<XText>()*/
                                ));





            }

            outXml.Save(output);

        }

        public static void Smooth(Stream first, Stream output, int interval)
        {

            TimeSpan dt = TimeSpan.FromSeconds(interval);
            XNamespace ns = "http://www.w3.org/ns/ttml";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            var firstXml = XDocument.Load(first);
            //var query = from c in firstXml.Descendants(ns + "p")
            //            select c;

            var start = (from b in firstXml.Descendants(ns + "p")
                         select DateTime.ParseExact(b.Attribute("begin").Value.ToString(), "HH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture))
                        .Aggregate((curMin, x) => (curMin == null || ((DateTime?)x ?? DateTime.MaxValue) < curMin ? x : curMin));

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
            IEnumerable<XElement> query;
            for (DateTime d = start; (query = (from e in firstXml.Descendants(ns + "p")
                                               let begin = DateTime.ParseExact(e.Attribute("begin").Value.ToString(), "HH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture)
                                               let end = DateTime.ParseExact(e.Attribute("end").Value.ToString(), "HH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture)
                                               where end > d && begin < d + dt // equals to (begin >= d && begin < d+dt) || (end > d && end <= d + dt) || (begin < d && end > d + dt)
                                               select e)).Count() > 0;
                 d += dt)
            {

                string cdata = "";
                foreach (XElement c in query)
                    cdata += c.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA).Parent.Value.Trim();
                XDocument xdoc = XDocument.Parse("<root>" + cdata + "</root>");


                IEnumerable<XElement> ff = from XElement e in xdoc.Descendants("Face").Nodes()
                        .Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))
                        .GroupBy(g => ((XElement)g).Name.ToString(), v => (string.IsNullOrEmpty(((XElement)v).Value) ? null : (double?)Convert.ToDouble(((XElement)v).Value, CultureInfo.InvariantCulture)))
                        .Select(g => new XElement(g.Key, g.Where(v => v!=null&&Math.Abs((double)v) < GlobalVar.MaxAccountedVal).Average().ToString()))
                                           select e;
                var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                        .Select(a => new { x = Convert.ToDouble(a[0], CultureInfo.InvariantCulture), y = Convert.ToDouble(a[1], CultureInfo.InvariantCulture) });

                var cd = new XDocument(new XElement("root"));
                if (ff.Any()) cd.Element("root").Add(new XElement("Face", new XAttribute("id", "0"), ff));

                string gazemark=null;
                if (gaze.Any())
                {
                    XElement gg = new XElement("Gaze", gaze.Average(g => g.x).ToString() + " " + gaze.Average(g => g.y).ToString());
                    cd.Descendants("Face").FirstOrDefault().Add(gg);
                    gazemark = gaze.Average(g => g.x).ToString() + "% " + gaze.Average(g => g.y).ToString() + "%";
                }

                IEnumerable<XElement> pp = from XElement e in xdoc.Descendants("Person").Nodes()
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value, CultureInfo.InvariantCulture))
                    .Select(g => new XElement(g.Key, g.Average().ToString()))
                                           select e;
                if (pp.Any())
                    cd.Element("root").Add(new XElement("Person", new XAttribute("id", "0"), pp));

                //O(Nln(N))
                var emotion = (from XElement p in query
                               group p by (p.Nodes().OfType<XText>().FirstOrDefault() == null ? "" : p.Nodes().OfType<XText>().FirstOrDefault().ToString().Trim()) into gp
                               orderby gp.Count() descending
                               select gp)
                               .Take(1)
                               .Select(g => g.Key);

                /*O(N)
                var emotions = (from XElement p in query
                               group p by (p.Nodes().OfType<XText>().FirstOrDefault() == null?"": p.Nodes().OfType<XText>().FirstOrDefault().ToString().Trim()) into gp
                               select new { gp.Key, c = gp.Count() });
                int max = emotions.Max(e => e.c);
                var emotion = emotions.Where(e => e.c == max).FirstOrDefault();
                               */
                cdata = "";
                foreach (XElement ee in cd.Descendants("root").Nodes())
                {
                    cdata += ee.ToString();
                }
                content.Add(new XElement(ns + "p", new XAttribute("begin", d.ToString("HH:mm:ss.ff", System.Globalization.CultureInfo.InvariantCulture)),
                                    new XAttribute("end", (d + dt).ToString("HH:mm:ss.ff", System.Globalization.CultureInfo.InvariantCulture)),
                                    new XElement(ns + "data", new XAttribute("type", "text/plain; charset = us-ascii"),
                                        new XElement(ns + "metadata", new XAttribute("id", (mid++).ToString())),
                                        new XCData(cdata)
                                    ),
                                    gazemark==null?null:new XElement(ns + "span", new XAttribute(tts+"origin",gazemark),new XText("+")),
                                    emotion
                                    
                                    /*,
                                        c.Descendants(ns + "span"),
                                        e.Descendants(ns + "span"),
                                        c.Nodes().OfType<XText>(),
                                        e.Nodes().OfType<XText>()*/
                                ));

            }
            outXml.Save(output);

        }

    }
}