using System;
using HtmlAgilityPack;
using WebPad.Rendering;
using WebPad.UserControls;

namespace WebPad.File;

public static class SaveToHtmlHandler
{
    
        public static void Save(SnippetDocumentControl snippet, string filePath)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            
            // use newlines with Stringbuilder so the document formats better than using @""
            var node = HtmlAgilityPack.HtmlNode.CreateNode(
                new System.Text.StringBuilder()
                    .AppendLine("<html>")
                    .AppendLine("<head></head>")
                    .AppendLine($"<body>{snippet.Html}</body>")
                    .AppendLine("</html>")
                    .ToString()
            );
            doc.DocumentNode.AppendChild(node);
            
            var head = doc.DocumentNode.SelectSingleNode("//head");
            var body = doc.DocumentNode.SelectSingleNode("//body");
            
            // save CSS
            if (!string.IsNullOrWhiteSpace(snippet.CSS))
            {
                var s = doc.CreateElement("style");
                s.SetAttributeValue("type", "text/css");
                s.AppendChild(doc.CreateTextNode(snippet.CSS));
                head.AppendChild(s);
            }
            
            // save javascript
            if (!string.IsNullOrWhiteSpace(snippet.Javascript))
            {
                var s = doc.CreateElement("script");
                s.SetAttributeValue("type", "text/javascript");
                s.AppendChild(doc.CreateTextNode(snippet.Javascript));
                body.AppendChild(s);
            }
            
            AppendSnippetReferences(snippet,doc, head);

            doc.Save(filename: filePath);
        }

        private static void AppendSnippetReferences(SnippetDocumentControl snippet, HtmlAgilityPack.HtmlDocument doc, HtmlNode head)
        {
            // take care of javascript references
            foreach (var reference in snippet.References)
            {
                if (reference.Type == ReferenceTypes.Css)
                {
                    var s = doc.CreateElement("link");
                    s.SetAttributeValue("rel", "stylesheet");
                    s.SetAttributeValue("href", reference.Url);
                    head.AppendChild(s);
                }
                else if (reference.Type == ReferenceTypes.Javascript)
                {
                    var s = doc.CreateElement("script");
                    s.SetAttributeValue("src", reference.Url);
                    s.SetAttributeValue("type", "text/javascript");
                    head.AppendChild(s);
                }
                else
                {
                    throw new Exception($"Unsupported snippet reference type: {reference.Type}");
                }
            }
        }
}