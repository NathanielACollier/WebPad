using System;
using System.Diagnostics;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using WebPad.CodeCompletion;
using WebPad.Rendering;
using System.Threading.Tasks;
using System.Threading;

namespace WebPad.UserControls
{
    /// <summary>
    /// Interaction logic for ActiveTabDocument.xaml
    /// </summary>
    public partial class SnippetDocumentControl : UserControl
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private readonly CodeCompletionBase _htmlCompletion;
        private readonly CodeCompletionBase _javascriptCompletion;
        private readonly CodeCompletionBase _css3SelectorCompletion;

        private WebServer.WebPadServerManager _internalServer;


        // this is where references are stored an executed at

        public SnippetDocumentControl()
        {
            InitializeComponent();

            try
            {
                References = new Rendering.References();
                References.AddDefaults();

                _css3SelectorCompletion = new CSSCodeCompletion(txtCSS);
                _htmlCompletion = new HtmlCodeCompletion(txtHtml);
                _javascriptCompletion = new JQueryCodeCompletion(txtJavascript);
            }
            catch (Exception e)
            {
                //todo: Need to improve the error handling!!
                Debug.WriteLine(e);
            }

            txtHtml.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("HTML");
            txtJavascript.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");
            txtCSS.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("CSS");


            txtHtml.MouseDown += TxtHtml_MouseDown;


            txtHtml.TextArea.MouseDown += TextArea_MouseDown;

            txtHtml.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            var pos = txtHtml.CaretOffset;
            var line = txtHtml.Document.GetLineByOffset(pos);
            int posInLine = pos - line.Offset;
            int lineNumber = line.LineNumber;

            log.Info($"Caret position changed in text editor.  [Line={lineNumber}, CharAt={posInLine}]");
        }

        private void TextArea_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            log.Info("Mouse down in text area");
        }

        private async void TxtHtml_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            log.Info("Mouse down in text editor");
            await GetElementRefFromHtmlEditorCurrentCaret();
        }


        private Task<int> GetElementRefFromHtmlEditorCurrentCaret()
        {
            var promise = new TaskCompletionSource<int>();

            var currentCaret = this.txtHtml.CaretOffset;
            var doc = this.txtHtml.Document;

            var t = new Thread(() =>
            {
                try
                {
                    var ch = doc.GetCharAt(currentCaret);
                    var line = doc.GetLineByOffset(currentCaret);
                    

                    promise.SetResult(0);
                }catch(Exception ex)
                {
                    log.Error($"Error getting element ref.  Exception: {ex}");
                    promise.SetResult(-1);
                }
            });
            t.Start();

            return promise.Task;
        }


        public Boolean IsModified
        {
            get { return txtHtml.IsModified || txtJavascript.IsModified || txtCSS.IsModified; }
            set { txtHtml.IsModified = value; txtJavascript.IsModified = value; txtCSS.IsModified = value; }
        }

        // keep up with the save file path so that we can save back to a file
        public string SaveFilePath { get; set; }

        // we are going to use the filename for the tab header
        public string SaveFileName
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(SaveFilePath);
            }
        }

        public string Html
        {
            get { return txtHtml.Text; }
            set { txtHtml.Text = value; }
        }

        public Rendering.References References { get; set; }


        public string ExternalHtmlPath { get; set; }

        public string BaseHref { get; set; } 

        public string Javascript
        {
            get { return txtJavascript.Text; }
            set { txtJavascript.Text = value; }
        }

        public string CSS
        {
            get { return txtCSS.Text; }
            set { txtCSS.Text = value; }
        }

        /// <summary>
        /// Returns back url of server, and makes sure it is running
        /// </summary>
        /// <returns></returns>
        public string EnsureServer()
        {
            if (this._internalServer == null)
            {
                log.Info("Creating new Web Server");
                this._internalServer = new WebServer.WebPadServerManager(this);
            }

            if(!this._internalServer.IsRunning)
            {
                this._internalServer.Start();
                log.Info("Starting web server");
            }

            log.Info($"Web server url is: {this._internalServer.Url}");
            return this._internalServer.Url;
        }

    }
}
