using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WebPad.Dependencies.General.WPFUserControls.LocalFolderBrowser
{
    public class MainModel : WPFViewModelBase.ViewModelBase
    {

        public bool Busy
        {
            get { return base.GetValue(() => this.Busy); }
            set { base.SetValue(() => this.Busy, value); }
        }



        public ObservableCollection<FolderModel> RootFolders
        {
            get { return base.GetValue(() => this.RootFolders); }
        }




    }
}
