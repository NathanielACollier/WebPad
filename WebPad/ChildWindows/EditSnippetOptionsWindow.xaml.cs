using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            setExternalHtmlPathFromRelativePath(_docControl.ExternalHtmlPath);
            context.BaseHref = _docControl.BaseHref;

            populateListOfRecentHtmlSnippets().ContinueWith((t) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    foreach( var entry in t.Result)
                    {
                        AddRecentHtmlSnippetSnippetToModel(entry);
                    }
                });
            });
        }

        public WebPad.UserControls.SnippetDocumentControl DocumentControl { get; set; }


        private void setExternalHtmlPathFromRelativePath( string relativePath)
        {
            var model = this.DataContext as EditSnippetOptionsWindowModel;
            var absolute = Utilities.PathUtilities.MakeAbsolutePath(full: this.DocumentControl.SaveFilePath, relative: this.DocumentControl.ExternalHtmlPath);

            model.ExternalHtmlTemplatePath = absolute;
        }


        private void OpenOrChangeFileButton_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as EditSnippetOptionsWindowModel;
            var dialog = new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = "html",
                Filter = "HTML (*.html)|*.html"
            };

            if(System.IO.File.Exists(model.ExternalHtmlTemplatePath))
            {
                var folderPath = System.IO.Path.GetDirectoryName(model.ExternalHtmlTemplatePath);
                dialog.InitialDirectory = folderPath;
            }

            if(dialog.ShowDialog()==true &&
                !string.Equals(model.ExternalHtmlTemplatePath, dialog.FileName, StringComparison.OrdinalIgnoreCase)
                )
            {
                model.ExternalHtmlTemplatePath = dialog.FileName;
                handleAddingRecentSnippetToModel();
                saveExternalHtmlSetting();
            }
        }


        private Task<List<Models.RecentHtmlSnippet>> populateListOfRecentHtmlSnippets()
        {
            string basePath = this.DocumentControl.SaveFilePath;
            var p = new TaskCompletionSource<List<Models.RecentHtmlSnippet>>();

            var t = new Thread(() =>
            {
                if(string.IsNullOrWhiteSpace(basePath))
                {
                    var snippets = Utilities.DBManager.GetAllRecentHtmlSnippets();

                    p.SetResult(snippets.ToList());
                }else
                {
                    var snippets = Utilities.DBManager.GetAllRecentHTMLSnippetsForBasePath(basePath);
                    p.SetResult(snippets.ToList());
                }

            });

            t.Start();

            return p.Task;
        }


        private void AddRecentHtmlSnippetSnippetToModel(Models.RecentHtmlSnippet snippet)
        {
            var model = this.DataContext as EditSnippetOptionsWindowModel;

            // setup events if any
            snippet.OnOpen += (_s, _args) =>
            {
                model.ExternalHtmlTemplatePath = snippet.FilePath;
                saveExternalHtmlSetting();
            };
            
            model.RecentSnippets.Add(snippet);
        }



        private void saveExternalHtmlSetting()
        {
            var context = this.DataContext as EditSnippetOptionsWindowModel;
            if (System.IO.File.Exists(this.DocumentControl.SaveFilePath))
            {
                if (System.IO.File.Exists(context.ExternalHtmlTemplatePath))
                {
                    this.DocumentControl.ExternalHtmlPath = Utilities.PathUtilities.MakeRelativePath(this.DocumentControl.SaveFilePath, context.ExternalHtmlTemplatePath);

                    this.DocumentControl.Html = System.IO.File.ReadAllText(context.ExternalHtmlTemplatePath);
                }
                else
                {
                    log.Error($"External html file path [{context.ExternalHtmlTemplatePath}] does not exist");
                }

            }
        }






        private async Task handleAddingRecentSnippetToModel()
        {
            var model = this.DataContext as EditSnippetOptionsWindowModel;

            var s = new Models.RecentHtmlSnippet
            {
                BaseFilePath = this.DocumentControl.SaveFilePath,
                FilePath = model.ExternalHtmlTemplatePath,
                FileName = System.IO.Path.GetFileName(model.ExternalHtmlTemplatePath)
            };

            if( await addRecentHtmlSnippet(s) > 0)
            {
                AddRecentHtmlSnippetSnippetToModel(s);
            }
        }


        private Task<int> addRecentHtmlSnippet(Models.RecentHtmlSnippet s)
        {
            var p = new TaskCompletionSource<int>();

            var t = new Thread(() =>
            {
                if(Utilities.DBManager.AddRecentHtmlSnippetIfNotDuplicate(s))
                {
                    p.SetResult(1);
                }else
                {
                    p.SetResult(-1);
                }
            });

            t.Start();

            return p.Task;
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
