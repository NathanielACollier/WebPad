using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WebPad.Dependencies.General.WPFViewModelBase;

namespace WebPad.Models
{
    public class RecentFileModel : ViewModelBase
    {

        public string FileName
        {
            get { return GetValue(() => this.FileName); }
            set { SetValue(() => this.FileName, value); }
        }

        public string Path
        {
            get { return GetValue(() => this.Path); }
            set { SetValue(() => this.Path, value); }
        }

        public File.SaveHandler.SaveType Type
        {
            get { return GetValue(() => this.Type); }
            set { SetValue(() => this.Type, value); }
        }


        public ICommand OpenRecentFileCommand
        {
            get {
                return new RelayCommand(win =>
                {
                    var window = (MainWindow)win;
                    window.OpenRecentFile(this);
                });
            }
        }



    }
}
