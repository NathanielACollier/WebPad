using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPad.Dependencies.General.WPFViewModelBase;

namespace WebPad
{
    public class MainWindowModel : ViewModelBase
    {
        public ObservableCollection<Models.RecentFileModel> RecentFiles
        {
            get { return GetValue(() => this.RecentFiles); }
        }
    }
}
