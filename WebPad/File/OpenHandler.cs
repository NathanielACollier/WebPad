using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebPad.UserControls;
using WebPad.Persistence;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;

namespace WebPad.File
{
    public class OpenHandler
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private static OpenFileDialog CreateOpenWebPadDialog()
        {
            return new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = "web",
                Filter = "WebPad Snippet (*.web)|*.web"
            };
        }


        private static OpenFileDialog CreateOpenHTMLDialog()
        {
            return new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = "html",
                Filter = "HTML (*.html)|*.html"
            };
        }




        private static SnippetDocumentControl GetSnippetControlFromSnippet( Snippet snippetData, string webPadFilePath = null)
        {
            SnippetDocumentControl snippet = null;

            if (snippetData != null)
            {
                snippet = new SnippetDocumentControl
                {
                    CSS = snippetData.CSS,
                    Html = snippetData.Html,
                    Javascript = snippetData.Javascript,
                    References = snippetData.References,
                    ExternalHtmlPath = snippetData.externalHtmlFile,
                    SaveFilePath = webPadFilePath,
                    BaseHref = snippetData.baseHref
                };

                if( System.IO.File.Exists(webPadFilePath) && !string.IsNullOrWhiteSpace(snippetData.externalHtmlFile))
                {
                    var fullPath = Utilities.PathUtilities.MakeAbsolutePath(webPadFilePath, snippetData.externalHtmlFile);

                    if(System.IO.File.Exists(fullPath))
                    {
                        snippet.Html = System.IO.File.ReadAllText(fullPath);
                    }else
                    {
                        log.Error($"External html file {fullPath} does not exist");
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Unable to parse data from {0}", webPadFilePath));
            }

            return snippet;
        }

        public static SnippetDocumentControl ParseSnippetFromWebPadXML(XDocument doc)
        {
            var snippetData = Utilities.XmlSerialization.Deserialize<Snippet>(doc);

            return GetSnippetControlFromSnippet(snippetData);
        }

        private static SnippetDocumentControl ParseSnippetFromWebPadFile(string webPadFilePath)
        {
            var doc = XDocument.Load(webPadFilePath);
            var snippetData = Utilities.XmlSerialization.Deserialize<Snippet>(doc);

            return GetSnippetControlFromSnippet(snippetData, webPadFilePath);
        }


        public static SnippetDocumentControl Open(SnippetDocumentControl currentDoc, SaveHandler.SaveType type)
        {
            OpenFileDialog dialog = null;

            if (type == SaveHandler.SaveType.HTML)
            {
                dialog = CreateOpenHTMLDialog();
            }
            else if (type == SaveHandler.SaveType.WebPad)
            {
                dialog = CreateOpenWebPadDialog();
            }

            // set initial folder to be where our current file is, if we have one
            if (System.IO.File.Exists(currentDoc.SaveFilePath))
            {
                var folderPath = System.IO.Path.GetDirectoryName(currentDoc.SaveFilePath);
                dialog.InitialDirectory = folderPath;
            }

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return null;

            return Open(type, dialog.FileName);
        }

        public static SnippetDocumentControl Open(SaveHandler.SaveType type, string filePath)
        {
            SnippetDocumentControl snippet = null;

            if (type == SaveHandler.SaveType.HTML)
            {
                snippet = Parsing.HTMLToSnippet.Parse(filePath);
            }
            else
            {
                snippet = ParseSnippetFromWebPadFile(filePath);
            }

            // if there are no references add in the defaults
            if (!snippet.References.Any())
            {
                snippet.References.AddDefaults();
            }

            // we just opened it so it isn't modified
            snippet.IsModified = false;

            return snippet;
        }


        public static SnippetDocumentControl Open( string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath);

            if (string.Equals(extension, ".html", StringComparison.OrdinalIgnoreCase))
            {
                // open html
                log.Info($"Opening html file {filePath}");
                var snippet = File.OpenHandler.Open(File.SaveHandler.SaveType.HTML, filePath);
                return snippet;
            }
            else if (string.Equals(extension, ".web", StringComparison.OrdinalIgnoreCase))
            {
                // open web
                log.Info($"Opening web file {filePath}");
                var snippet = File.OpenHandler.Open(File.SaveHandler.SaveType.WebPad, filePath);
                return snippet;
            }

            throw new Exception($"Unsupported file type {extension}");
        }



    }
}
