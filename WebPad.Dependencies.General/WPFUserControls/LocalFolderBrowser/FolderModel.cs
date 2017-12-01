using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WebPad.Dependencies.General.WPFUserControls.LocalFolderBrowser
{
    public class FolderModel : WPFViewModelBase.ViewModelBase
    {

        public string Name
        {
            get { return this.GetValue(() => this.Name); }
            set { this.SetValue(() => this.Name, value); }
        }


        public string Path
        {
            get { return this.GetValue(() => this.Path); }
            set { this.SetValue(() => this.Path, value); }
        }


        public bool Busy
        {
            get { return this.GetValue(() => this.Busy); }
            set { this.SetValue(() => this.Busy, value); }
        }


        public ObservableCollection<FolderModel> SubFolders
        {
            get { return this.GetValue(() => this.SubFolders); }
        }

    }
}
