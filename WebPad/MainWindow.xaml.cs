using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using WebPad.CodeCompletion;
using WebPad.Persistence;
using WebPad.Preferences;
using WebPad.Properties;
using WebPad.Rendering;
using WebPad.UserControls;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;
using System.Diagnostics;

using NCWPFExtensions;

namespace WebPad
{
    public partial class MainWindow : IUserPreferences
    {

        private readonly HtmlRenderer _resultsRenderer;

        #region Commands and Binding
        // edit menu
        public static readonly RoutedCommand CommandReferences = new RoutedCommand();
        public static readonly RoutedCommand CommandEditFileOptions = new RoutedCommand();

        // other
        public static readonly RoutedCommand CommandBuildExecute = new RoutedCommand();

        // File Commands
        public static readonly RoutedCommand CommandExit = new RoutedCommand();

        public static readonly RoutedCommand CommandSaveAsWebPad = new RoutedCommand();

        // window commands
        public static readonly RoutedCommand CommandNextPane = new RoutedCommand();
        public static readonly RoutedCommand CommandHideResultsPane = new RoutedCommand();
        public static readonly RoutedCommand CommandHideHelpPane = new RoutedCommand();
        public static readonly RoutedCommand CommandHideEditorPane = new RoutedCommand();


        // browser commands
        public static readonly RoutedCommand CommandBrowserIE = new RoutedCommand();
        public static readonly RoutedCommand CommandBrowserChrome = new RoutedCommand();
        public static readonly RoutedCommand CommandBrowserFirefox = new RoutedCommand();
        public static readonly RoutedCommand CommandBrowserEdge = new RoutedCommand();

        

        // html commands
        public static readonly RoutedCommand CommandOpenFromHTML = new RoutedCommand();
        public static readonly RoutedCommand CommandSaveToHTML = new RoutedCommand();

        public static readonly RoutedCommand CommandSaveAsHTML = new RoutedCommand();

        private readonly CanExecuteRoutedEventHandler _canAlwaysExecute = (sender, e) => e.CanExecute = true;

        private void BindCommands()
        {
            // edit menu
            MenuReferences.Command = CommandReferences;
            CommandBindings.Add(new CommandBinding(CommandReferences, ExecuteReferences));
            CommandReferences.InputGestures.Add(new KeyGesture(Key.F4));


            MenuEditFileOptions.Command = CommandEditFileOptions;
            CommandBindings.Add(new CommandBinding(CommandEditFileOptions, ExecuteEditFileOptions));

            // other

            MenuExecute.Command = CommandBuildExecute;
            CommandBindings.Add(new CommandBinding(CommandBuildExecute, ExecuteBuild));
            CommandBuildExecute.InputGestures.Add(new KeyGesture(Key.F5));

            // file commands
            MenuExit.Command = CommandExit;
            CommandBindings.Add(new CommandBinding(CommandExit, ExecuteExit));
            CommandExit.InputGestures.Add(new KeyGesture(Key.F4, ModifierKeys.Alt));

            MenuSaveAsWebPad.Command = CommandSaveAsWebPad;
            CommandBindings.Add(new CommandBinding(CommandSaveAsWebPad, ExecuteSaveAsWebPad));
            

            // window commands
            MenuNextPane.Command = CommandNextPane;
            CommandBindings.Add(new CommandBinding(CommandNextPane, ExecuteNextPane));
            CommandNextPane.InputGestures.Add(new KeyGesture(Key.F6));

            MenuHideResultsPane.Command = CommandHideResultsPane;
            CommandBindings.Add(new CommandBinding(CommandHideResultsPane, ExecuteHideResultsPane));
            CommandHideResultsPane.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));

            MenuHideEditorPane.Command = CommandHideEditorPane;
            CommandBindings.Add(new CommandBinding(CommandHideEditorPane, ExecuteHideEditorPane));
            CommandHideEditorPane.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));



            // open in browser commands
            MenuBrowserIE.Command = CommandBrowserIE;
            CommandBindings.Add(new CommandBinding(CommandBrowserIE, ExecuteBrowserIE));

            MenuBrowserChrome.Command = CommandBrowserChrome;
            CommandBindings.Add(new CommandBinding(CommandBrowserChrome, ExecuteBrowserChrome));

            MenuBrowserFirefox.Command = CommandBrowserFirefox;
            CommandBindings.Add(new CommandBinding(CommandBrowserFirefox, ExecuteBrowserFirefox));

            MenuBrowserEdge.Command = CommandBrowserEdge;
            CommandBindings.Add(new CommandBinding(CommandBrowserEdge, ExecuteBrowserEdge));



            // save/open html
            MenuOpenFromHTML.Command = CommandOpenFromHTML;
            CommandBindings.Add(new CommandBinding(CommandOpenFromHTML, ExecuteOpenFromHTML));

            MenuSaveToHTML.Command = CommandSaveToHTML;
            CommandBindings.Add(new CommandBinding(CommandSaveToHTML, ExecuteSaveToHTML));

            MenuSaveAsHTML.Command = CommandSaveAsHTML;
            CommandBindings.Add(new CommandBinding(CommandSaveAsHTML, ExecuteSaveAsHTML));


            // built in commands

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, ExecuteNew, _canAlwaysExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, ExecuteSave, _canAlwaysExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, ExecuteOpen, _canAlwaysExecute));
        }

        #endregion

        private int newQueryCount = 0;

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            Log4NetHelpers.CodeConfiguredUtilities.InitializeLog4Net();
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Images/globe.ico", UriKind.RelativeOrAbsolute));


            _resultsRenderer = new HtmlRenderer(grdInternetExplorerResults);

            AddLogTab();
            log.Info("Application Started");

            var newSnippet = CreateNewDocument();
            
            BindCommands();

            Closing += OnClosing;

            LoadPreferences(Settings.Default);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var editedCount = DocumentTab.Items
                                    .Cast<TabItem>()
                                    .Where(tab=>tab.Content is SnippetDocumentControl)
                                    .Select(tab => tab.Content as SnippetDocumentControl)
                                    .Where(snippet => snippet.IsModified).Count();

            if (editedCount <= 0) 
                return;

            if (MessageBox.Show(String.Format("There {0} with unsaved changes. Are you sure you want to exit?", 
                editedCount == 1 ? "is a tab" : "are " + editedCount + " tabs"),
                "Unsaved Changes", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }


        private void AddLogTab()
        {
            var headerTemplate = this.FindResource("logTabHeader") as DataTemplate;
            var contentTemplate = this.FindResource("logTabContent") as DataTemplate;

            var tabItem = new TabItem();
            DocumentTab.Items.Insert(0, tabItem);

            var headerCtrl = headerTemplate.LoadContent() as StackPanel;
            var bodyCtrl = contentTemplate.LoadContent() as NacWPFControls.LogViewer.Log4NetViewerControl;

            tabItem.Header = headerCtrl;
            tabItem.Content = bodyCtrl;
        }


        private SnippetDocumentControl CreateNewDocument(SnippetDocumentControl bodyCtrl=null)
        {
            // may need to use a deserialized snippet control as a clone for a new document so there are things we need to do to clean it up
            if( bodyCtrl != null)
            {
                bodyCtrl.SaveFilePath = "";
            }
            return AddSnippetTab(bodyCtrl: bodyCtrl, tabName: $"New Query {newQueryCount++}");
        }


        private SnippetDocumentControl AddSnippetTab(  SnippetDocumentControl bodyCtrl = null, string tabName = null)
        {
            var headerTemplate = this.FindResource("docTabHeader") as DataTemplate;
            

            var tabItem = new TabItem();
            DocumentTab.Items.Insert(0, tabItem);
            DocumentTab.SelectedItem = tabItem;

            var headerCtrl = headerTemplate.LoadContent() as StackPanel;

            if(bodyCtrl == null)
            {
                var contentTemplate = this.FindResource("docTabContent") as DataTemplate;
                bodyCtrl = contentTemplate.LoadContent() as SnippetDocumentControl;
            }
            
            tabItem.Header = headerCtrl;
            tabItem.Content = bodyCtrl;

            UpdateHeaderOfSnippetDocumentControl(bodyCtrl, tabName);

            return bodyCtrl;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            SavePreferences(Settings.Default);
            Settings.Default.Save();
        }


        #region Command Execute Events


        private SnippetDocumentControl GetSelectedTabSnippetDocumentControl()
        {
            var currentTab = DocumentTab.SelectedItem as TabItem;

            if( currentTab != null && currentTab.Content is SnippetDocumentControl)
            {
                var currentDoc = currentTab.Content as SnippetDocumentControl;

                return currentDoc;
            }

            return null;
        }


        private void ExecuteEditFileOptions(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();

            if( currentDoc == null)
            {
                log.Error("Current tab is not a snippet document control so we cannot edit file options");
                return;
            }

            var dialog = new ChildWindows.EditSnippetOptionsWindow(currentDoc);
            dialog.ShowDialog();

        }

        private void ExecuteReferences(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();

            if ( currentDoc== null)
            {
                log.Error("Current tab is not a snippet document control so we cannot open referenced");
                return;
            }

            var referencesDialog = new ReferencesWindow(currentDoc.References);
            referencesDialog.ShowDialog();
            currentDoc.References = referencesDialog.GetReferences();
           System.Windows.Forms.MessageBox.Show(currentDoc.References.GetHtml());
        }

        private void ExecuteBuild(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();

            if( currentDoc == null)
            {
                log.Error("Current tab is not a snippet document control so we cannot build");
                return;
            }

            log.Info("Rendering");
            _resultsRenderer.Render(currentDoc);
        }


        // File Exit Command
        private void ExecuteExit(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }




        

        private void ExecuteBrowser(Action<string> cmdWithURL)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();

            if( currentDoc == null)
            {
                log.Error("Current tab is not a snippet document control so we cannot Execute Browser");
                return;
            }

            try
            {
                string url = currentDoc.EnsureServer();

                cmdWithURL(url);
            }
            catch( Exception ex )
            {
                System.Windows.MessageBox.Show($"Failed to launch browser.  Exception: {ex}", "Failed to navigate browser", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void LaunchBrowserAtUrl( string browserPath, string url)
        {
            if(!System.IO.File.Exists(browserPath))
            {
                System.Windows.MessageBox.Show($"Browser at path: {browserPath} does not exist.", "Failed to launch browser", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var p = new Process();
            p.StartInfo.FileName = browserPath;
            p.StartInfo.Arguments = $"{url}";
            p.Start();
        }

        private void ExecuteBrowserIE(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteBrowser((url) =>
            {
                LaunchBrowserAtUrl(Settings.Default.InternetExplorerEXEPath, url);
            });
        }

        private void ExecuteBrowserChrome(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteBrowser((url)=>
            {
                LaunchBrowserAtUrl(Settings.Default.ChromeEXEPath, url);
            });
        }

        private void ExecuteBrowserFirefox(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteBrowser((url)=>
            {
                LaunchBrowserAtUrl(Settings.Default.FireFoxEXEPath, url);
            });
        }

        private void ExecuteBrowserEdge(object sender, ExecutedRoutedEventArgs e)
        {
            // edge is different than the others
            ExecuteBrowser(cmdWithURL: (url) =>
            {
                Process.Start($"microsoft-edge:{url}");
            });
        }


        private static void ExecuteNextPane(object sender, ExecutedRoutedEventArgs e)
        {
            //if (txtHtml.IsKeyboardFocusWithin)
            //    txtJavascript.Focus();
            //else if (txtJavascript.IsKeyboardFocusWithin)
            //    txtHtml.Focus();
            //else if (grdResults.IsKeyboardFocusWithin)
            //    grdResults.Focus();
            //else if (grdInstantHelp.IsKeyboardFocusWithin)
            //    grdInstantHelp.Focus();
            //else
            //    txtHtml.Focus();
        }

        private void ExecuteHideResultsPane(object sender, ExecutedRoutedEventArgs e)
        {
            ResultDefinition.Height = (ResultDefinition.Height.IsStar ? new GridLength(0) : new GridLength(CalculateNewPaneHeight(), GridUnitType.Star));
            ResultSplitterDefinition.Height = (ResultSplitterDefinition.Height.Value == 0 ? new GridLength(5) : new GridLength(0));
            ResultSplitterDefinition.MinHeight = ResultSplitterDefinition.MinHeight == 0 ? 5 : 0;
            MenuHideResultsPane.Header = ResultDefinition.Height.Value == 0 ? "Sh_ow Results Pane" : "Hi_de Results Pane";
        }


        private void ExecuteHideEditorPane(object sender, ExecutedRoutedEventArgs e)
        {
            EditorsDefinition.Height = (EditorsDefinition.Height.IsStar ? new GridLength(0) : new GridLength(CalculateNewPaneHeight(), GridUnitType.Star));
            ResultSplitterDefinition.Height = (ResultSplitterDefinition.Height.Value == 0 ? new GridLength(5) : new GridLength(0));
            ResultSplitterDefinition.MinHeight = ResultSplitterDefinition.MinHeight == 0 ? 5 : 0;
            MenuHideEditorPane.Header = EditorsDefinition.Height.Value == 0 ? "Sh_ow Editor Pane" : "Hi_de Editor Pane";
        }


        /// <summary>
        /// When the results or help panes are made visible after being hidden we need a way to re-calculate the space they should
        /// occupy since, when they were hidden, the others would have filled up the space that remained.
        /// </summary>
        /// <returns></returns>
        private double CalculateNewPaneHeight()
        {
            double newHeight = 0;

            if (ResultDefinition.Height.Value == 0 && EditorsDefinition.Height.Value == 0)
            {
                newHeight = 300;
            }
            else if (ResultDefinition.Height.Value == 0 )
            {
                newHeight = EditorsDefinition.Height.Value / 2;
            }
            else if (EditorsDefinition.Height.Value == 0)
            {
                newHeight = ResultDefinition.Height.Value / 2;
            }

            
            //return ResultDefinition.Height.Value == 0 ? EditorsDefinition.Height.Value / 2 : ResultDefinition.Height.Value/2;
            return newHeight;
        }

        private void ExecuteNew(object sender, ExecutedRoutedEventArgs e)
        {
            CreateNewDocument();
        }



        private void UpdateHeaderOfSnippetDocumentControl(SnippetDocumentControl ctrl, string tabName = null)
        {

            //.SaveFileName
            TabItem tab = ctrl.FindVisualParent<TabItem>();
            var headerCtrl = tab.Header as FrameworkElement;
            var queryNameTB = headerCtrl.FindNameDownTree("queryNameTextBlock") as TextBlock;
            if (tabName == null )
            {
                // don't blank out the name if there is no save file
                if(!string.IsNullOrWhiteSpace(ctrl.SaveFilePath))
                {
                    queryNameTB.Text = ctrl.SaveFileName;
                }
                
            }else
            {
                queryNameTB.Text = tabName;
            }
        }


        private void ExecuteSave(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();

            if( currentDoc == null)
            {
                log.Error("Cannot save selected tab is not a snippet");
                return;
            }

            try
            {
                File.SaveHandler.Save(currentDoc, File.SaveHandler.SaveType.WebPad, false);
                UpdateHeaderOfSnippetDocumentControl(currentDoc);


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Failed to save web file.  Exception: {0}", ex), "Failed to save web file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ExecuteSaveAsHTML(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();

            if( currentDoc == null)
            {
                System.Windows.MessageBox.Show("Cannot save as HTML, no document to save in this tab.", "Failed to save", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                File.SaveHandler.Save(currentDoc, File.SaveHandler.SaveType.HTML, true);
                UpdateHeaderOfSnippetDocumentControl(currentDoc);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Failed to save as html.  Exception: {0}", ex), "Failed to save as HTML", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSaveAsWebPad(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();

            if( currentDoc == null)
            {
                System.Windows.MessageBox.Show("Cannot save as webpage, no document to save in this tab.", "Failed to save", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                File.SaveHandler.Save(currentDoc, File.SaveHandler.SaveType.WebPad, true);
                UpdateHeaderOfSnippetDocumentControl(currentDoc);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Failed to save as WebPad.  Exception: {0}", ex), "Failed to save as WebPad", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ExecuteOpen(object sender, ExecutedRoutedEventArgs e)
        {

            try
            {

                SnippetDocumentControl snippet = File.OpenHandler.Open(File.SaveHandler.SaveType.WebPad);
                if (snippet != null)
                {
                    AddSnippetTab(snippet);
                } else
                {
                    log.Info("Didn't open webpad file, no snippet was found in file");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Failed to open file.  Exception: {0}", ex), "Failed to Open Web File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void ExecuteOpenFromHTML(object sender, ExecutedRoutedEventArgs e)
        {


            try
            {
                SnippetDocumentControl snippet = File.OpenHandler.Open(File.SaveHandler.SaveType.HTML);

                if (snippet != null)
                {
                    AddSnippetTab(snippet);
                }
                else
                {
                    log.Info("Didn't open html, no snippet was found in file");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Failed to load html file.  With Exception {0}",ex), "HTML File Open Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void ExecuteSaveToHTML(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();
            if( currentDoc == null)
            {
                System.Windows.MessageBox.Show("Failed to save to html, current tab doesn't have a document.", "Failed to save", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                File.SaveHandler.Save(currentDoc, File.SaveHandler.SaveType.HTML, false);
                UpdateHeaderOfSnippetDocumentControl(currentDoc);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Failed to save html file.  Exception: {0}", ex), "Failed Saving HTML", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #endregion

        #region IUserPreferences

        public void LoadPreferences(Settings settings)
        {
            Top = settings.WindowTop;
            Left = settings.WindowLeft;
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;
            WindowState = settings.WindowState;
        }

        public void SavePreferences(Settings settings)
        {
            if (WindowState != WindowState.Minimized)
            {
                settings.WindowTop = Top;
                settings.WindowLeft = Left;
                settings.WindowWidth = Width;
                settings.WindowHeight = Height;
                settings.WindowState = WindowState;
            }
        }

        #endregion

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            var closeButton = sender as System.Windows.Controls.Button;
            var tab = closeButton.FindVisualParent<TabItem>();

            if( tab.Content is SnippetDocumentControl)
            {
                var document = tab.Content as SnippetDocumentControl;

                if (document.IsModified &&
                    System.Windows.MessageBox.Show("You have modified text.  You should save first before closing.  Click cancel to save first.  Click ok to continue on without saving.  You may loose work!!!", "Loose Changes?", MessageBoxButton.OKCancel, MessageBoxImage.Warning)
                        != System.Windows.MessageBoxResult.OK
                    )
                {
                    return;
                }

                DocumentTab.Items.Remove(tab);

                var snippetControlTabs = DocumentTab.Items
                                        .OfType<TabItem>()
                                        .Where(t => t.Content is SnippetDocumentControl);
                if (snippetControlTabs.Any())
                {
                    // just select it, an event handler will take care of everything else
                    snippetControlTabs
                        .Last().IsSelected = true;

                }
                else
                {
                    CreateNewDocument();
                }

            }// end of snippet document control specific content
        }// end of close tab button



        private TabItem FindTabItemFromContextMenuParentPlacementTarget(DependencyObject obj)
        {
            try
            {
                var contextMenu = obj.FindVisualParent<System.Windows.Controls.ContextMenu>();

                var target = contextMenu.PlacementTarget;

                var tabItem = target.FindVisualParent<TabItem>();

                return tabItem;
            }
            catch( Exception ex)
            {
                log.Error($"Exception searching for Tab Item.  Exception: {ex}");
            }

            return null;
        }

        private void MenuCloneScript_Click(object sender, RoutedEventArgs e)
        {
            var cloneMenuItem = sender as System.Windows.Controls.MenuItem;
            var tab = FindTabItemFromContextMenuParentPlacementTarget(cloneMenuItem);

            if( tab == null)
            {
                System.Windows.MessageBox.Show("Something went wrong trying to locate the tab you want to clone.  An application error may be in place.  Check log and report problem.", "Unable to find tab to clone", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if( tab.Content is SnippetDocumentControl)
            {
                var doc = tab.Content as SnippetDocumentControl;

                try
                {
                    // use serialization to clone the tab
                    var xmlDoc = File.SaveHandler.SaveToXDocument(doc);

                    var clone = File.OpenHandler.ParseSnippetFromWebPadXML(xmlDoc);

                    CreateNewDocument(bodyCtrl: clone);
                }
                catch( Exception ex)
                {
                    log.Error($"Failed to clone tab.  Exception: {ex}");
                }
            }
        }// end of clone script menuitem

        private void folderBrowser_FileDoubleClicked(object sender, CodeRunner.FolderBrowser.FileSelectedEventArgs e)
        {
            try
            {
                var snippet = File.OpenHandler.Open(e.SelectedFile.Path);

                AddSnippetTab(snippet);
            }
            catch( Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load file from folder browser.  Exception: {ex}", "Failed to load file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
