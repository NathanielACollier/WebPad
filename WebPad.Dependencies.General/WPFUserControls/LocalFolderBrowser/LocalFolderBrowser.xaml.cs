using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;

using WebPad.Dependencies.General.Extensions.WPF;


using System.Threading;
using System.Collections.ObjectModel;

namespace WebPad.Dependencies.General.WPFUserControls.LocalFolderBrowser
{



    /// <summary>
    /// Interaction logic for LocalFolderBrowser.xaml
    /// </summary>
    public partial class LocalFolderBrowser : UserControl
    {
        public delegate void FolderSelectedHandler(object sender, FolderSelectedEventArgs e);
        public event FolderSelectedHandler FolderSelected;

        public delegate void FileDoubleClickedHandler(object sender, FileSelectedEventArgs e);
        public event FileDoubleClickedHandler FileDoubleClicked;

        public delegate void FileSelectedHandler(object sender, FileSelectedEventArgs e);
        public event FileSelectedHandler FileSelected;


        #region SelectedFolder DP

        public static readonly DependencyProperty SelectedFolderProperty = DependencyProperty.Register("SelectedFolder", typeof(FolderModel), typeof(LocalFolderBrowser));

        public FolderModel SelectedFolder
        {
            get { return (FolderModel)GetValue(SelectedFolderProperty); }
            set { SetValue(SelectedFolderProperty, value); }
        }


        #endregion

        #region FileExtensions DP

        public static readonly DependencyProperty FileExtensionsProperty = DependencyProperty.Register("FileExtensions", typeof(string), typeof(LocalFolderBrowser));

        public string FileExtensions
        {
            get { return (string)GetValue(FileExtensionsProperty); }
            set { SetValue(FileExtensionsProperty, value); }
        }

        public IEnumerable<string> FileExtensionsList
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.FileExtensions))
                {
                    return this.FileExtensions
                            .Split(',', ';', ' ', '\t', '\n')
                            .Where(str => !string.IsNullOrWhiteSpace(str))
                            .Select(str =>
                            {
                                // get rid of leading '.'
                                if (str[0] == '.')
                                {
                                    return str.Substring(1);
                                }
                                else
                                {
                                    return str;
                                }
                            });
                }
                else
                {
                    return new string[] { };
                }

            }
        }

        #endregion


        #region Model DP

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(MainModel), typeof(LocalFolderBrowser));

        public MainModel Model
        {
            get { return this.GetValueThreadSafe<MainModel>(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }


        #endregion



        public LocalFolderBrowser()
        {
            InitializeComponent();

            this.Model = new MainModel();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                AddRootFolders();
            }
        }

        /// <summary>
        /// This function is used to add folders to any sub folder, and also to add folders to the root list of folders
        /// </summary>
        /// <param name="subFolders"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        private void AddFolderThreadSafe(ObservableCollection<FileSystemNodeModel> subFolders, string name, string path)
        {
            var folder = new FolderModel
            {
                Name = name,
                Path = path
            };
            // add an empty child, so we can expand it
            folder.Children.Add(new FolderModel());

            this.Dispatcher.Invoke(() =>
            {
                subFolders.Add(folder);
            });
        }

        private void AddFileThreadSafe(ObservableCollection<FileSystemNodeModel> items, string name, string path)
        {
            var file = new FileModel
            {
                Name = name,
                Path = path
            };

            this.Dispatcher.Invoke(() =>
            {
                items.Add(file);
            });
        }


        private void AddRootFolders()
        {
            Thread t = new Thread(() =>
            {
                try
                {

                    // create special folders (Top level Desktop)

                    string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                    AddFolderThreadSafe(Model.RootFolders, "Desktop", desktopPath);


                    string myDocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                    AddFolderThreadSafe(Model.RootFolders, "My Documents", myDocumentsPath);


                    string userProfilePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);


                    string linksFolderPath = System.IO.Path.Combine(userProfilePath, "Links"); // this is where Favorites that appears in Windows Explorer is
                    AddFolderThreadSafe(Model.RootFolders, "Favorites", linksFolderPath);


                    AddFolderThreadSafe(Model.RootFolders, System.Environment.UserName, userProfilePath);

                    foreach (var drive in System.IO.DriveInfo.GetDrives())
                    {
                        AddFolderThreadSafe(Model.RootFolders, drive.Name, drive.Name);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(string.Format("Failed to add root folders.  Exception: {0}", ex), "Failed to add root folders", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    this.Model.Busy = false;
                }
            });

            this.Model.Busy = true;
            t.Start();
        }



        private void PopulateChildrenThreadSafe(FolderModel parentFolder)
        {

            // first clear what's there
            this.Dispatcher.Invoke(() =>
            {
                parentFolder.Children.Clear();
            });


            if (System.IO.Directory.Exists(parentFolder.Path))
            {
                // enumerate shortcuts
                foreach (string shortcutFile in System.IO.Directory.EnumerateFiles(parentFolder.Path, "*.lnk", System.IO.SearchOption.TopDirectoryOnly))
                {

                    // alot of win32 encapsulated stuff is going on here, so stick it in a try/catch
                    try
                    {
                        string shortcutName = System.IO.Path.GetFileNameWithoutExtension(shortcutFile);
                        string resolvedPath = Utilities.FileSystemShortcutHandler.ResolveShortcut(shortcutFile);

                        // determining directories from this information: http://stackoverflow.com/questions/439447/net-how-to-check-if-path-is-a-file-and-not-a-directory
                        // determine if it's a directory
                        var fileAttrs = System.IO.File.GetAttributes(resolvedPath);

                        // fileAttrs is a bitmask, so we have to use the & bitwise operater to determine if it's a directory or not
                        if ((fileAttrs & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                        {
                            AddFolderThreadSafe(parentFolder.Children, shortcutName, resolvedPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // shortcut didn't work out (But I don't think we need an error message)
                    }
                }

                // enumerate directories
                foreach (string directoryPath in System.IO.Directory.EnumerateDirectories(parentFolder.Path, "*.*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    var dirInfo = new System.IO.DirectoryInfo(directoryPath);

                    AddFolderThreadSafe(parentFolder.Children, dirInfo.Name, directoryPath);
                }

                List<string> extensions = new List<string>();

                this.Dispatcher.Invoke(() =>
                {
                    // we can't access the DP inside this thread, so copy it
                    extensions.AddRange(this.FileExtensionsList);
                });

                // enumerate files
                foreach (string filePath in System.IO.Directory.EnumerateFiles(parentFolder.Path, "*.*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    var fileInfo = new System.IO.FileInfo(filePath);

                    if (extensions.Any() == false ||
                        extensions.Any(ext => string.Equals(fileInfo.Extension, '.' + ext, StringComparison.OrdinalIgnoreCase))
                       )
                    {
                        AddFileThreadSafe(parentFolder.Children, fileInfo.Name, filePath);
                    }

                }// end of looping through files
            }// end of if the folder exists


        }// end PopulateChildrenThreadSafe function


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="parentFolderTreeItem">Send this so that if a folder didn't have sub folders, we could de-expand it</param>
        private void PopulateChildren(FolderModel parentFolder, TreeViewItem parentFolderTreeItem)
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    PopulateChildrenThreadSafe(parentFolder);
                }
                catch (Exception ex)
                {
                    // ignore folders that had exceptions (They will de-expand)
                }
                finally
                {
                    parentFolder.Busy = false;

                    if (!parentFolder.Children.Any())
                    {
                        // add back a sub folder and de-expand
                        this.Dispatcher.Invoke(() =>
                        {
                            parentFolder.Children.Add(new FolderModel());
                            parentFolderTreeItem.IsExpanded = false; // de-expand so that the user could try expanding it again (If they put something in the folder or whatever...)
                        });
                    }
                }

            });

            parentFolder.Busy = true;
            t.Start();
        }

        private void TreeView_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;

            if (item != null)
            {
                var folder = item.DataContext as FolderModel;

                if (folder != null)
                {
                    PopulateChildren(folder, item);
                }
            }
        }



        private void OpenSelectedFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FolderModel selectedFolder = (sender as MenuItem).DataContext as FolderModel;

            if (selectedFolder != null)
            {
                System.Diagnostics.Process.Start("explorer.exe", selectedFolder.Path);
            }
        }

        private void FolderContentControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderModel selectedFolder = (sender as ContentControl).DataContext as FolderModel;

            if (selectedFolder != null)
            {
                this.SelectedFolder = selectedFolder;

                if (this.FolderSelected != null)
                {
                    this.FolderSelected(this, new FolderSelectedEventArgs
                    {
                        SelectedFolder = selectedFolder
                    });
                }
            }
        }

        private void FolderContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void FileContentControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ctrl = sender as ContentControl;
            var file = ctrl.DataContext as FileModel;

            if (this.FileSelected != null)
            {
                this.FileSelected(this, new FileSelectedEventArgs
                {
                    SelectedFile = file
                });
            }
        }

        private void FileContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var ctrl = sender as ContentControl;
            var file = ctrl.DataContext as FileModel;

            if (this.FileDoubleClicked != null)
            {
                this.FileDoubleClicked(this, new FileSelectedEventArgs
                {
                    SelectedFile = file
                });
            }
        }

        private void OpenSelectedFileMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }




}
