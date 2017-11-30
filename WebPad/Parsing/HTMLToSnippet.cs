using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebPad.Parsing
{
    public static class HTMLToSnippet
    {

        public static WebPad.UserControls.SnippetDocumentControl Parse(string filePath)
        {
            WebPad.UserControls.SnippetDocumentControl snippet = new UserControls.SnippetDocumentControl();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            doc.Load(filePath);

            snippet.References = GetReferences(doc);
            snippet.Javascript = GetJavaScript(doc);
            snippet.CSS = GetStyleSheets(doc);
            snippet.SaveFilePath = filePath;
            snippet.Html = GetBodyHTML(doc);

            return snippet;
        }

        private static WebPad.Rendering.References GetReferences(HtmlAgilityPack.HtmlDocument doc)
        {
            WebPad.Rendering.References references = new Rendering.References();

            var cssReferences = doc.DocumentNode.Descendants("link")
                                        .Where(node => node.Attributes.Contains("href") &&
                                                        node.Attributes.Contains("type") &&
                                                        string.Equals(node.Attributes["type"].Value, "text/css", StringComparison.OrdinalIgnoreCase)
                                                )
                                        .Select(node => new WebPad.Rendering.Reference
                                        {
                                            Type = Rendering.ReferenceTypes.Css,
                                            Url = node.Attributes["href"].Value
                                        })
                                        .ToList();

            var javascriptReferences = doc.DocumentNode.Descendants("script")
                                            .Where(node => node.Attributes.Contains("src") &&
                                                        node.Attributes.Contains("type") &&
                                                        string.Equals(node.Attributes["type"].Value, "text/javascript", StringComparison.OrdinalIgnoreCase)
                                                    )
                                            .Select(node => new WebPad.Rendering.Reference
                                            {
                                                Type = Rendering.ReferenceTypes.Javascript,
                                                Url = node.Attributes["src"].Value
                                            })
                                            .ToList();

            foreach( var reference in cssReferences )
            {
                references.Add(reference);
            }

            foreach (var reference in javascriptReferences)
            {
                references.Add(reference);
            }


            return references;
        }



        private static string GetJavaScript(HtmlAgilityPack.HtmlDocument doc)
        {
            var javascriptSections = doc.DocumentNode.Descendants("script")
                                            .Where(node => !node.Attributes.Contains("src") &&
                                                        node.Attributes.Contains("type") &&
                                                        string.Equals(node.Attributes["type"].Value, "text/javascript", StringComparison.OrdinalIgnoreCase)
                                                )
                                            .Select(node => node.InnerText);

            return string.Join(Environment.NewLine, javascriptSections);
        }


        private static string GetStyleSheets(HtmlAgilityPack.HtmlDocument doc)
        {
            var styleSheetSections = doc.DocumentNode.Descendants("style")
                                             .Where(node => node.Attributes.Contains("type") &&
                                                        string.Equals(node.Attributes["type"].Value, "text/css", StringComparison.OrdinalIgnoreCase)
                                                    )
                                            .Select(node => node.InnerText);

            return string.Join(Environment.NewLine, styleSheetSections);
        }


        private static string GetBodyHTML(HtmlAgilityPack.HtmlDocument doc)
        {
            var bodySections = doc.DocumentNode.Descendants("body")
                                            .Select(node => node.InnerHtml);

            return string.Join(Environment.NewLine, bodySections);
        }


    }
}
