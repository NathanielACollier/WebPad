﻿using System;
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


using Log4NetHelpers= WebPad.Dependencies.General.Log4NetHelpers;
using LogViewer = WebPad.Dependencies.General.WPFUserControls.LogViewer;
using LocalFolderBrowser = WebPad.Dependencies.General.WPFUserControls.LocalFolderBrowser;

using WebPad.Dependencies.General.Extensions.WPF;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

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

        public static readonly RoutedCommand CommandSaveAs = new RoutedCommand();

        // window commands
        public static readonly RoutedCommand CommandNextPane = new RoutedCommand();
        public static readonly RoutedCommand CommandHideResultsPane = new RoutedCommand();
        public static readonly RoutedCommand CommandHideEditorPane = new RoutedCommand();


        // browser commands
        public static readonly RoutedCommand CommandBrowserIE = new RoutedCommand();
        public static readonly RoutedCommand CommandBrowserChrome = new RoutedCommand();
        public static readonly RoutedCommand CommandBrowserFirefox = new RoutedCommand();
        public static readonly RoutedCommand CommandBrowserEdge = new RoutedCommand();
        

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

            MenuSaveAs.Command = CommandSaveAs;
            CommandBindings.Add(new CommandBinding(CommandSaveAs, ExecuteSaveAs));


            

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
            MenuBrowserOpenDefault.Command = CommandBrowserIE;
            CommandBindings.Add(new CommandBinding(CommandBrowserIE, ExecuteBrowserDefault));

            // built in commands

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, ExecuteNew, _canAlwaysExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, ExecuteSave, _canAlwaysExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, ExecuteOpen, _canAlwaysExecute));
        }

        #endregion

        private int newQueryCount = 0;


        #region Model DP

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(MainWindowModel), typeof(MainWindow));

        public MainWindowModel Model
        {
            get { return this.GetValueThreadSafe<MainWindowModel>(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        #endregion

        nac.Database.SQLite.Database db = null;

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            this.Model = new MainWindowModel();

            populateListOfRecentFiles().ContinueWith((t) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    foreach (var entry in t.Result)
                    {
                        AddRecentFileToModel(entry);
                    }
                });

            });


            Log4NetHelpers.CodeConfiguredUtilities.InitializeLog4Net();
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Images/globe.ico", UriKind.RelativeOrAbsolute));


            _resultsRenderer = new HtmlRenderer(grdInternetExplorerResults);

            _resultsRenderer.WebBrowserElementClicked += _resultsRenderer_WebBrowserElementClicked;

            AddLogTab();
            log.Info("Application Started");

            var newSnippet = CreateNewDocument();
            
            BindCommands();

            Closing += OnClosing;

            LoadPreferences(Settings.Default);
        }






        private Task<List<Models.RecentFileModel>> populateListOfRecentFiles()
        {
            var p = new TaskCompletionSource<List<Models.RecentFileModel>>();

            var t = new Thread(() =>
            {
                var files = Utilities.DBManager.GetAllRecentFiles();

                p.SetResult(files.ToList());
            });

            t.Start();


            return p.Task;
        }


        private void AddRecentFileToModel(Models.RecentFileModel file)
        {
            file.OnOpen += (_sender, _args) =>
            {
                this.OpenRecentFile(_sender as Models.RecentFileModel);
            };

            file.OnRename += (_sender, _args) =>
            {
                this.RenameRecentFile(_sender as Models.RecentFileModel);
            };

            file.OnRemove += (_sender, _args) =>
            {
                this.RemoveFromRecentFiles(_sender as Models.RecentFileModel);
            };

            file.OnClearAll += (_sender, _args) =>
            {
                this.ClearAllRecentFiles();
            };

            this.Model.RecentFiles.Add(file);
        }



        public void OpenRecentFile(Models.RecentFileModel file)
        {
            SnippetDocumentControl snippet = File.OpenHandler.Open( file.Path);

            if ( snippet != null)
            {
                AddSnippetTab(snippet);
            }
        }


        public void RenameRecentFile(Models.RecentFileModel file)
        {
            var form = new nac.wpf.forms.Form()
                            .TextBoxFor("FileName", file.FileName)
                            .Display();

            if( !string.Equals(form.Model["FileName"] as string, file.FileName))
            {
                Utilities.DBManager.RenameRecentFile(file, form.Model["FileName"] as string);
                file.FileName = form.Model["FileName"] as string;
            }

        }


        public void RemoveFromRecentFiles(Models.RecentFileModel file)
        {
            Utilities.DBManager.RemoveRecentFile(file);
            this.Model.RecentFiles.Remove(file);
        }


        public void ClearAllRecentFiles()
        {
            Utilities.DBManager.ClearAllRecentFiles();
            this.Model.RecentFiles.Clear();
        }
        
        

        private async Task handleAddingRecentFile(Models.RecentFileModel file)
        {
            if( await addRecentFile(file) > 0)
            {
                // it was a new recent file so add it to model
                AddRecentFileToModel(file);
            }
        }


        private Task<int> addRecentFile(Models.RecentFileModel file)
        {
            var p = new TaskCompletionSource<int>();

            var t = new Thread(() =>
            {

                if(Utilities.DBManager.AddRecentFileIfNotDuplicate(file))
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



        private void _resultsRenderer_WebBrowserElementClicked(object _sender, WebBrowserElementClickedEventArgs args)
        {
            // get current snippet doc
            var currentDocCtrl = GetSelectedTabSnippetDocumentControl();

            if( currentDocCtrl != null)
            {
                currentDocCtrl.SetHTMLEditorPosition(args.LineNumber, args.Column);
            }
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
            var bodyCtrl = contentTemplate.LoadContent() as LogViewer.Log4NetViewerControl;

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


            bodyCtrl.HtmlEditorCaretPositionChangeDelayedEvent += CurrentSnippetDocument_HtmlEditorCaretPositionChangeDelayedEvent;
            bodyCtrl.HtmlEditorTextChangeDelayedEvent += CurrentSnippetDocument_HtmlEditorTextChangeDelayedEvent;

            return bodyCtrl;
        }

        private void CurrentSnippetDocument_HtmlEditorTextChangeDelayedEvent(object sender, EventArgs args)
        {
            var docCtrl = sender as UserControls.SnippetDocumentControl;

            if( docCtrl != null)
            {
                var selectedDoc = this.GetSelectedTabSnippetDocumentControl();

                if( selectedDoc != null)
                {
                    // are we on the doc that fired the event???
                    if( docCtrl != selectedDoc)
                    {
                        docCtrl.HtmlEditorTextChangeDelayedEvent -= CurrentSnippetDocument_HtmlEditorTextChangeDelayedEvent;
                    }

                    // render what we have
                    _resultsRenderer.Render(selectedDoc).ContinueWith(t =>
                    {
                        log.Info("Render completed");
                    });
                }
            }
        }

        private void CurrentSnippetDocument_HtmlEditorCaretPositionChangeDelayedEvent(object sender, HtmlEditorCaretPositionChangeEventArgs args)
        {
            var docCtrl = sender as UserControls.SnippetDocumentControl;

            if( docCtrl != null)
            {
                var selectedDoc = this.GetSelectedTabSnippetDocumentControl();

                if( selectedDoc != null)
                {

                    if( docCtrl != selectedDoc)
                    {
                        // turn the event off, it's not this doc
                        docCtrl.HtmlEditorCaretPositionChangeDelayedEvent -= CurrentSnippetDocument_HtmlEditorCaretPositionChangeDelayedEvent;
                    }

                    // scroll the web browser to the line?
                    this._resultsRenderer.ScrollBrowserTo(args.LineNumber, args.Column);
                }
            }
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
            _resultsRenderer.Render(currentDoc).ContinueWith(t =>
            {
                log.Info("Render completed");
            });
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
        

        private void ExecuteBrowserDefault(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteBrowser((url) =>
            {
                OpenUrl(url);
            });
        }
        
        /*
         from: https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
         */
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
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
                File.SaveHandler.Save(currentDoc, false);
                UpdateHeaderOfSnippetDocumentControl(currentDoc);


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Failed to save web file.  Exception: {0}", ex), "Failed to save web file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        
        private void ExecuteSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            var currentDoc = GetSelectedTabSnippetDocumentControl();

            if( currentDoc == null)
            {
                System.Windows.MessageBox.Show("Cannot save as, no document to save in this tab.", "Failed to save as", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                File.SaveHandler.Save(currentDoc, true);
                UpdateHeaderOfSnippetDocumentControl(currentDoc);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save as.  Exception: {ex}", "Failed to save as", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        private void ExecuteOpen(object sender, ExecutedRoutedEventArgs e)
        {

            try
            {

                // determine if we have a file open already
                var currentDoc = GetSelectedTabSnippetDocumentControl();

                SnippetDocumentControl snippet = File.OpenHandler.Open(currentDoc);
                if (snippet != null)
                {
                    handleAddingRecentFile(new Models.RecentFileModel
                    {
                        Path = snippet.SaveFilePath,
                        FileName = snippet.SaveFileName
                    });

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

        private void folderBrowser_FileDoubleClicked(object sender, LocalFolderBrowser.FileSelectedEventArgs e)
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

        private void RecentFileClearAllItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            db.Command("delete from RecentFiles");
            this.Model.RecentFiles.Clear();
        }



    }
}
