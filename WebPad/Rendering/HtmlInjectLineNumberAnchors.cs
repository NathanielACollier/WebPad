using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Rendering
{
    public static class HtmlInjectLineNumberAnchors
    {
        public static string cssClassMarker = "webpadHasLineNumber";

        public static string Inject(string htmlText)
        {
            //https://stackoverflow.com/questions/9520932/how-do-i-use-html-agility-pack-to-edit-an-html-snippet

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlText);

            foreach( var node in doc.DocumentNode.Descendants().Where(n=>n.NodeType == HtmlAgilityPack.HtmlNodeType.Element))
            {
                var classAttr = node.Attributes["class"];

                if( classAttr != null)
                {
                    classAttr.Value = $"{classAttr.Value} {cssClassMarker}";
                }else
                {
                    node.SetAttributeValue("class", cssClassMarker);
                }

                node.Attributes.Add("lineNumber", node.Line.ToString());
                node.Attributes.Add("column", node.LinePosition.ToString());
                
            }

            using (StringWriter writer = new StringWriter())
            {
                doc.Save(writer);
                var result = writer.ToString();
                return result;
            }
            
        }
    }
}
