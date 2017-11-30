using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebPad.UserControls;
using System.Windows.Forms;
using WebPad.Persistence;
using System.Xml.Serialization;
using WebPad.Rendering;

namespace WebPad.File
{
    public class SaveHandler
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);



        public enum SaveType
        {
            HTML, WebPad
        };


        private static SaveFileDialog CreateSaveHTMLDialog()
        {
            return new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "html",
                Filter = "HTML (*.html)|*.html"
            };
        }

        private static SaveFileDialog CreateSaveWebPadDialog()
        {
            return new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "web",
                Filter = "WebPad Snippet (*.web)|*.web"
            };
        }


        private static void SaveToWebFile(SnippetDocumentControl snippetControl, string filePath)
        {
            var doc = SaveToXDocument(snippetControl);
            log.Info($"Saving to {filePath}");
            doc.Save(filePath);

            if(!string.IsNullOrWhiteSpace(snippetControl.ExternalHtmlPath))
            {
                var fullExtHtmlPath = Utilities.PathUtilities.MakeAbsolutePath(filePath, snippetControl.ExternalHtmlPath);
                log.Info($"Saving html to external document: [relative={snippetControl.ExternalHtmlPath},full={fullExtHtmlPath}]");

                if(System.IO.File.Exists(fullExtHtmlPath))
                {
                    // save all the html to it
                    System.IO.File.WriteAllText(fullExtHtmlPath, snippetControl.Html);
                }else
                {
                    log.Error($"External html file: [{fullExtHtmlPath}] does not exist");
                }
            }
        }

        public static System.Xml.Linq.XDocument SaveToXDocument( SnippetDocumentControl snippetControl)
        {
            var snippetData = new Snippet(snippetControl);

            var doc = Utilities.XmlSerialization.Serialize<Snippet>(snippetData);

            return doc;
        }


        private static void SaveToHTMLFile(SnippetDocumentControl snippet, string filePath)
        {
            var documentText = HtmlTemplate.GetDocumentText(snippet);

            System.IO.File.WriteAllText(filePath, documentText);
        }



        private static string GetNewFilePath(SnippetDocumentControl snippet, SaveType type)
        {
            SaveFileDialog dialog = null;

            if (type == SaveType.HTML)
            {
                dialog = CreateSaveHTMLDialog();
            }
            else if (type == SaveType.WebPad)
            {
                dialog = CreateSaveWebPadDialog();
            }

            if (dialog.ShowDialog() != DialogResult.OK)
                return null;

            return dialog.FileName;
        }



        private static Boolean CanSaveToExistingFile(SnippetDocumentControl snippet, string extensionRequired)
        {
            if (System.IO.File.Exists(snippet.SaveFilePath) &&
                string.Equals(System.IO.Path.GetExtension(snippet.SaveFilePath), extensionRequired, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snippet"></param>
        /// <param name="type"></param>
        /// <param name="saveAs">If false an attempt will be made to save the file back to it's original name or a new name if the file doesn't exist.
        ///                         If true it will save it to a new name no matter what
        ///                         This should enable save as functionality</param>
        public static void Save(SnippetDocumentControl snippet, SaveType type, bool saveAs)
        {
            string filePath;
            bool newFile = false;

            if (!saveAs)
            {
                // determine if the file exists
                if (type == SaveType.WebPad && !CanSaveToExistingFile(snippet, ".web"))
                {
                    newFile = true;
                }
                else if (type == SaveType.HTML && !CanSaveToExistingFile(snippet, ".html"))
                {
                    newFile = true;
                }
            }
            else
            {
                newFile = true;
            }

            if (newFile)
            {
                filePath = GetNewFilePath(snippet, type);
            }
            else
            {
                filePath = snippet.SaveFilePath;
            }

            // make sure the filepath is not null or empty, because that might indicate the user canceled out of saving and we want to go back
            if (!string.IsNullOrEmpty(filePath))
            {

                if (type == SaveType.HTML)
                {
                    SaveToHTMLFile(snippet, filePath);
                }
                else if (type == SaveType.WebPad)
                {
                    SaveToWebFile(snippet, filePath);
                }

                snippet.SaveFilePath = filePath; // this filepath may have changed
                snippet.IsModified = false;

            }
        }
    }
}
