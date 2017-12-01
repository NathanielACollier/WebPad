using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WebPad.Dependencies.General.WPFUserControls.LocalFolderBrowser
{
    public class FolderModel : FileSystemNodeModel
    {


        public bool Busy
        {
            get { return this.GetValue(() => this.Busy); }
            set { this.SetValue(() => this.Busy, value); }
        }


        public ObservableCollection<FileSystemNodeModel> Children
        {
            get { return this.GetValue(() => this.Children); }
        }

    }
}
