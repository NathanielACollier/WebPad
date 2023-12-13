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
        private static nac.Logging.Logger log = new();



        public enum SaveType
        {
            HTML, WebPad
        };

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



        private static string GetNewFilePath(SnippetDocumentControl snippet)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "html",
                Filter = "HTML (*.html)|*.html|HTML (*.htm)|*.htm|WebPad Snippet (*.web)|*.web"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return null;

            return dialog.FileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snippet"></param>
        /// <param name="type"></param>
        /// <param name="saveAs">If false an attempt will be made to save the file back to it's original name or a new name if the file doesn't exist.
        ///                         If true it will save it to a new name no matter what
        ///                         This should enable save as functionality</param>
        public static void Save(SnippetDocumentControl snippet, bool saveAs)
        {
            string filePath;
            bool newFile = !System.IO.File.Exists(snippet.SaveFilePath);
            
            if (newFile || saveAs)
            {
                filePath = GetNewFilePath(snippet);
            }
            else
            {
                filePath = snippet.SaveFilePath;
            }

            // make sure the filepath is not null or empty, because that might indicate the user canceled out of saving and we want to go back
            if (!string.IsNullOrEmpty(filePath))
            {
                log.Info($"Saving file to {filePath}");
                string fileExt = System.IO.Path.GetExtension(filePath);
                
                if (new[] { ".html", ".htm" }.Contains(fileExt,
                        StringComparer.OrdinalIgnoreCase))
                {
                    File.SaveToHtmlHandler.Save(snippet, filePath);
                }else if (string.Equals(fileExt, ".web", StringComparison.OrdinalIgnoreCase))
                {
                    SaveToWebFile(snippet, filePath);
                }
                else
                {
                    throw new Exception($"File extension [ext={fileExt}] is not a supported file type");
                }

                snippet.SaveFilePath = filePath; // this filepath may have changed
                snippet.IsModified = false;
                log.Info("Saving file success");

            }
        }
        
        
        
        
        
    }
}
