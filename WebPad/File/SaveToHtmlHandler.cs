﻿using System;
using HtmlAgilityPack;
using WebPad.Rendering;
using WebPad.UserControls;

namespace WebPad.File;

public static class SaveToHtmlHandler
{
    
        public static void Save(SnippetDocumentControl snippet, string filePath)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            var node = HtmlAgilityPack.HtmlNode.CreateNode($@"
                <html>
                    <head></head>
                    <body>
                        {snippet.Html}
                    </body>
                </html>
            ");
            doc.DocumentNode.AppendChild(node);
            
            var head = doc.DocumentNode.SelectSingleNode("//head");
            var body = doc.DocumentNode.SelectSingleNode("//body");
            
            // save CSS
            if (!string.IsNullOrWhiteSpace(snippet.CSS))
            {
                head.AppendChild(
                    HtmlAgilityPack.HtmlNode.CreateNode(
                        "<style type='text/css'>" + Environment.NewLine + snippet.CSS + Environment.NewLine +"</style>"
                        )
                );
            }
            
            // save javascript
            if (!string.IsNullOrWhiteSpace(snippet.Javascript))
            {
                body.AppendChild(
                    HtmlAgilityPack.HtmlNode.CreateNode(
                        "<script type='text/javascript'>" + Environment.NewLine + snippet.Javascript +
                        Environment.NewLine + "</script>"
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
                        HtmlAgilityPack.HtmlNode.CreateNode($@"
                            <link rel='stylesheet' href='{reference.Url}'>
                        ")
                    );
                }
                else if (reference.Type == ReferenceTypes.Javascript)
                {
                    head.AppendChild(
                        HtmlAgilityPack.HtmlNode.CreateNode($@"
                            <script src='{reference.Url}' type='text/javascript'></script>
                        ")
                    );
                }
                else
                {
                    throw new Exception($"Unsupported snippet reference type: {reference.Type}");
                }
            }
        }
}