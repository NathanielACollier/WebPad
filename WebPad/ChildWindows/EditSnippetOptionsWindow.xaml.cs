using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WebPad.ChildWindows
{
    /// <summary>
    /// Interaction logic for EditSnippetOptionsWindow.xaml
    /// </summary>
    public partial class EditSnippetOptionsWindow : Window
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EditSnippetOptionsWindow(WebPad.UserControls.SnippetDocumentControl _docControl)
        {
            InitializeComponent();
            this.DocumentControl = _docControl; // keep a reference to this
            var context = this.DataContext as EditSnippetOptionsWindowModel;
            context.ExternalHtmlTemplatePath = _docControl.ExternalHtmlPath;
            context.BaseHref = _docControl.BaseHref;

            context.RecentSnippets.Add(new Models.RecentHtmlSnippet
            {
                BaseFilePath = "Duck Duck Goose",
                FilePath = "Goose",
                FileName = "Hello World"
            });
        }

        public WebPad.UserControls.SnippetDocumentControl DocumentControl { get; set; }

        private void ExternalHtmlTemplatePathFilePicker_FilePathChanged(object sender, EventArgs e)
        {
            // make sure window is initialized
            if( this.DocumentControl != null)
            {

                saveExternalHtmlSetting();
            }
            // end of file picker filepath changed
        }


        private void saveExternalHtmlSetting()
        {
            var context = this.DataContext as EditSnippetOptionsWindowModel;
            if (System.IO.File.Exists(this.DocumentControl.SaveFilePath))
            {
                if (System.IO.File.Exists(context.ExternalHtmlTemplatePath))
                {
                    this.DocumentControl.ExternalHtmlPath = Utilities.PathUtilities.MakeRelativePath(this.DocumentControl.SaveFilePath, context.ExternalHtmlTemplatePath);

                    var absolute = Utilities.PathUtilities.MakeAbsolutePath(full: this.DocumentControl.SaveFilePath, relative: this.DocumentControl.ExternalHtmlPath);
                    this.DocumentControl.Html = System.IO.File.ReadAllText(context.ExternalHtmlTemplatePath);
                }
                else
                {
                    log.Error($"External html file path [{context.ExternalHtmlTemplatePath}] does not exist");
                }

            }
        }

        private void BaseHrefTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var context = this.DataContext as EditSnippetOptionsWindowModel;

            if( this.DocumentControl != null)
            {
                saveBaseHref();
            }
        }

        private void saveBaseHref()
        {
            var context = this.DataContext as EditSnippetOptionsWindowModel;
            this.DocumentControl.BaseHref = context.BaseHref;
        }

        private void SaveOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            

            if( this.DocumentControl != null)
            {
                log.Info("Saving options");
                saveBaseHref();
                saveExternalHtmlSetting();
            }
        }
    }
}
