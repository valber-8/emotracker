using System;
using System.Collections.Generic;
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

        public static void Merge(FileStream first, FileStream second, FileStream output)
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
                if (c1.Count() > 0)
                {
                    var node = c1.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                    var cont = node.Parent.Value.Trim();
                    var xdoc2 = XDocument.Parse("<root>" + cont + "</root>");

                    foreach (XElement ee in xdoc.Descendants("Face"))
                    {
                        ee.Attribute("id").Value = "1_" + ee.Attribute("id").Value;
                    }
                    foreach (XElement ee in xdoc.Descendants("Person"))
                    {
                        ee.Attribute("id").Value = "1_" + ee.Attribute("id").Value;
                    }
                    xdoc.Descendants("root").FirstOrDefault().Add(xdoc2.Descendants("root").FirstOrDefault().Nodes());
                }
                if (c2.Count() > 0)
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
                }
                string cdata = "";
                foreach (XElement ee in xdoc.Descendants("root").Nodes())
                    cdata += ee.ToString();

                List<XElement> nodes = Enumerable.Empty<XElement>().ToList();
                if (c1.Count() > 0) nodes.Add(c1.Descendants(ns + "span").FirstOrDefault());
                if (c2.Count() > 0) nodes.Add(c2.Descendants(ns + "span").FirstOrDefault());
                List<XText> text = Enumerable.Empty<XText>().ToList();
                if (c1.Count() > 0) text.Add(c1.Nodes().OfType<XText>().FirstOrDefault());
                if (c2.Count() > 0) text.Add(c2.Nodes().OfType<XText>().FirstOrDefault());
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


            /*
            var query = from c in firstXml.Descendants(ns + "p")
                        select c;
            foreach (XElement c in query)
            {
                var nested1 = from e in secondXml.Descendants(ns + "p")
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
                              where DateTime.Parse(c.Attribute("begin").Value) >= DateTime.Parse(e.Attribute("begin").Value)
                                    && DateTime.Parse(c.Attribute("begin").Value) < DateTime.Parse(e.Attribute("end").Value)
                                    && DateTime.Parse(c.Attribute("end").Value) > DateTime.Parse(e.Attribute("begin").Value)
                                    && DateTime.Parse(c.Attribute("end").Value) <= DateTime.Parse(e.Attribute("end").Value)
                              select e;



                Action<XElement, String, String> contAdd = (e, begin, end) =>
                {
                        var node = c.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                        var cont = node.Parent.Value.Trim();
                        var xdoc = XDocument.Parse("<root>" + cont + "</root>");

                        foreach (XElement ee in xdoc.Descendants("Face"))
                        {
                            ee.Attribute("id").Value = "1_" + ee.Attribute("id").Value;
                        }
                        foreach (XElement ee in xdoc.Descendants("Person"))
                        {
                            ee.Attribute("id").Value = "1_" + ee.Attribute("id").Value;
                        }


                        node = e.Descendants(ns + "data").DescendantNodes().Single(el => el.NodeType == XmlNodeType.CDATA);
                        cont = node.Parent.Value.Trim();

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
                            cdata += ee.ToString();

                        content.Add(new XElement(ns + "p", new XAttribute("begin", begin), new XAttribute("end", end),
                                        new XElement(ns + "data", new XAttribute("type", "text/plain; charset = us-ascii"),
                                            new XElement(ns + "metadata", new XAttribute("id", (mid++).ToString())),               //change it
                                                                                                                                   //new XCData(xdoc.Descendants("root").DescendantNodes().ToString())
                                                                                                                                   //new XCData(string.Join("\n",xdoc.Descendants("root").DescendantNodes().ToString()))
                                            new XCData(cdata)
                                        ),
                                        c.Descendants(ns + "span"),
                                        e.Descendants(ns + "span"),
                                        c.Nodes().OfType<XText>(),
                                        e.Nodes().OfType<XText>()
                                    ));


                    
                };

                foreach (XElement e in nested1) contAdd(e, c.Attribute("begin").Value, e.Attribute("end").Value);
                foreach (XElement e in nested2) contAdd(e, e.Attribute("begin").Value, c.Attribute("end").Value);
                foreach (XElement e in nested3) contAdd(e, e.Attribute("begin").Value, e.Attribute("end").Value);
                foreach (XElement e in nested4) contAdd(e, c.Attribute("begin").Value, c.Attribute("end").Value);
                

            }
            */
            outXml.Save(output);

        }
        public static void Average(FileStream first, FileStream output)
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

                IEnumerable<XElement> ll = from XElement e in xdoc.Descendants("Face").Nodes()
                    .Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value))
                    .Select(g => new XElement(g.Key, g.Where(v => Math.Abs(v) < GlobalVar.MaxAccountedVal).Average().ToString()))
                                           select e;

                var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                     .Select(a => new { x = Convert.ToDouble(a[0]), y = Convert.ToDouble(a[1]) });
                var cd = new XDocument(new XElement("root", new XElement("Face", new XAttribute("id", "0"), ll)));

                if (gaze.Count() > 0)
                {
                    XElement gg = new XElement("Gaze", gaze.Average(g => g.x).ToString() + " " + gaze.Average(g => g.y).ToString());
                    cd.Descendants("Face").FirstOrDefault().Add(gg);
                }

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

        public static void Smooth(FileStream first, FileStream output, int interval)
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


                IEnumerable<XElement> ll = from XElement e in xdoc.Descendants("Face").Nodes()
                        .Where(ee => (((XElement)ee).Name.ToString() != "Landmark") && (((XElement)ee).Name.ToString() != "Gaze"))
                        .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value))
                        .Select(g => new XElement(g.Key, g.Where(v => Math.Abs(v) < GlobalVar.MaxAccountedVal).Average().ToString()))
                                           select e;
                var gaze = (from XElement e in xdoc.Descendants("Face").Nodes().Where(ee => (((XElement)ee).Name.ToString() == "Gaze"))
                            select e.Value.ToString().Split(' '))
                                        .Select(a => new { x = Convert.ToDouble(a[0]), y = Convert.ToDouble(a[1]) });

                var cd = new XDocument(new XElement("root", new XElement("Face", new XAttribute("id", "0"), ll)));
                if (gaze.Count() > 0)
                {
                    XElement gg = new XElement("Gaze", gaze.Average(g => g.x).ToString() + " " + gaze.Average(g => g.y).ToString());
                    cd.Descendants("Face").FirstOrDefault().Add(gg);
                }

                IEnumerable<XElement> pp = from XElement e in xdoc.Descendants("Person").Nodes()
                    .GroupBy(g => ((XElement)g).Name.ToString(), v => Convert.ToDouble(((XElement)v).Value))
                    .Select(g => new XElement(g.Key, g.Average().ToString()))
                                           select e;
                if (pp.Count() > 0)
                    cd.Element("root").Add(new XElement("Person", new XAttribute("id", "0"), pp));

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
                                    )/*,
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