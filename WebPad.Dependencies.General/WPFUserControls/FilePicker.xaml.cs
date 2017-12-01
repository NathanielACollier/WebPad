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

using System.IO;

namespace WebPad.Dependencies.General.WPFUserControls
{
    /// <summary>
    /// Interaction logic for FilePicker.xaml
    /// </summary>
    public partial class FilePicker : UserControl
    {

        #region FilePath DP
        /*
         * Remember that to bind to this Property you need to use the TwoWay mode if you want to write the values back to the property
         * Here is an example of this: <local:FilePicker FilePath="{Binding Path, Mode=TwoWay}"></local:DirectoryPicker>
         */
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            "FilePath",
            typeof(string),
            typeof(FilePicker),
            new UIPropertyMetadata(string.Empty, new PropertyChangedCallback(FilePathChangedCallBack)));

        /**
         * <summary>
         *  This property is both so that the user can determine if a directory has been picked, and so that they can get the value
         * </summary>
         */
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }



        /**
         * <summary>
         *  This fires when the DirectoryPath is changed so that we can set our textbox
         * </summary>
         */
        static void FilePathChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            // create a reference to our DirectoryPicker object that contains the DependencyProperty
            FilePicker fp = (FilePicker)sender;
            // our directorypath has changed at this point so raise an event
            fp.NotifyFilePathChanged();
        }




        #endregion




        #region FileMustExist DP

        public static readonly DependencyProperty FileMustExistProperty = DependencyProperty.Register(
            "FileMustExist",
            typeof(bool),
            typeof(FilePicker),
            new UIPropertyMetadata(true)
            );

        public bool FileMustExist
        {
            get { return (bool)this.GetValue(FileMustExistProperty); }
            set { this.SetValue(FileMustExistProperty, value); }
        }

        #endregion


        #region FileNameFilter DP

        public static readonly DependencyProperty FileNameFilterProperty = DependencyProperty.Register(
            "FileNameFilter",
            typeof(string),
            typeof(FilePicker),
            new UIPropertyMetadata(string.Empty)
            );

        public string FileNameFilter
        {
            get { return (string)this.GetValue(FileNameFilterProperty); }
            set { this.SetValue(FileNameFilterProperty, value); }
        }

        #endregion


        #region FileName DP
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register("FileName", typeof(string), typeof(FilePicker));

        /**
         * <summary>
         *   This should be the filename and the extension
         * </summary>
         */
        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        #endregion

        public event EventHandler FilePathChanged;

        private void NotifyFilePathChanged()
        {
            if (FilePathChanged != null)
            {
                FilePathChanged(this, new EventArgs());
            }
        }


        public FilePicker()
        {
            InitializeComponent();
        }








        private void PickExistingFile()
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();

            if (!string.IsNullOrEmpty(this.FileNameFilter))
            {
                fileDialog.Filter = this.FileNameFilter;
            }

            // see if there is an existing directory we can start in
            if (!string.IsNullOrEmpty(this.FilePath))
            {
                try
                {
                    string dirPath = System.IO.Path.GetDirectoryName(this.FilePath);

                    if (System.IO.Directory.Exists(dirPath))
                    {
                        fileDialog.InitialDirectory = dirPath;
                    }
                }
                catch (Exception ex) { }
            }
            
            System.Windows.Forms.DialogResult result = fileDialog.ShowDialog(Utilities.WPFWindowsFormsIntegrationUtil.GetIWin32Window(this));


            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FilePath = fileDialog.FileName;
                FileName = fileDialog.SafeFileName;
            }
            else
            {
                FilePath = string.Empty;
                FileName = string.Empty;
            }
        }



        private void PickNewFile()
        {
            Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog();
            fileDialog.OverwritePrompt = false; // is this right?  This will prevent the prompt telling the user they may overwrite an existing file

            if (!string.IsNullOrEmpty(this.FileNameFilter))
            {
                fileDialog.Filter = this.FileNameFilter;
            }

            // see if there is an existing directory we can start in
            if (!string.IsNullOrEmpty(this.FilePath))
            {
                try
                {
                    string dirPath = System.IO.Path.GetDirectoryName(this.FilePath);

                    if (System.IO.Directory.Exists(dirPath))
                    {
                        fileDialog.InitialDirectory = dirPath;
                    }
                }
                catch (Exception ex) { }
            }


            if (fileDialog.ShowDialog() == true)
            {
                this.FilePath = fileDialog.FileName;
                this.FileName = fileDialog.SafeFileName;
            }
            else
            {
                this.FilePath = string.Empty;
                this.FileName = string.Empty;
            }

        }


        /**
         * <summary>
         *  Uses OpenFileDialog to let the user pick a Directory
         * </summary>
         */
        private void PickFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.FileMustExist == true)
            {
                // pick existing file
                PickExistingFile();
            }
            else
            {
                // pick new file
                PickNewFile();
            }

        }

    }
}
