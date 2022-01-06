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
                    .AppendLine("<body>")
                    .AppendLine(snippet.Html)
                    .AppendLine("</body>")
                    .AppendLine("</html>")
                    .ToString()
            );
            doc.DocumentNode.AppendChild(node);
            
            var head = doc.DocumentNode.SelectSingleNode("//head");
            var body = doc.DocumentNode.SelectSingleNode("//body");
            
            // save CSS
            if (!string.IsNullOrWhiteSpace(snippet.CSS))
            {
                head.AppendChild(
                    HtmlAgilityPack.HtmlNode.CreateNode(
                        new System.Text.StringBuilder()
                            .AppendLine("<style type='text/css'>")
                            .AppendLine(snippet.CSS)
                            .AppendLine("</style>")
                            .ToString()
                    )
                );
            }
            
            // save javascript
            if (!string.IsNullOrWhiteSpace(snippet.Javascript))
            {
                body.AppendChild(
                    HtmlAgilityPack.HtmlNode.CreateNode(
                        new System.Text.StringBuilder()
                            .AppendLine("<script type='text/javascript'>" )
                            .AppendLine(snippet.Javascript)
                            .AppendLine("</script>")
                            .ToString()
                    )
                );
            }
            
            AppendSnippetReferences(snippet, head);

            doc.Save(filename: filePath);
        }

        private static void AppendSnippetReferences(SnippetDocumentControl snippet, HtmlNode head)
        {
            // take care of javascript references
            foreach (var reference in snippet.References)
            {
                if (reference.Type == ReferenceTypes.Css)
                {
                    head.AppendChild(
                        HtmlAgilityPack.HtmlNode.CreateNode(
                            $"<link rel='stylesheet' href='{reference.Url}'>"
                            )
                    );
                }
                else if (reference.Type == ReferenceTypes.Javascript)
                {
                    head.AppendChild(
                        HtmlAgilityPack.HtmlNode.CreateNode(
                            $"<script src='{reference.Url}' type='text/javascript'></script>"
                            )
                    );
                }
                else
                {
                    throw new Exception($"Unsupported snippet reference type: {reference.Type}");
                }
            }
        }
}