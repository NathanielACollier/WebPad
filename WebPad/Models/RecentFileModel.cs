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
        public event Action<object, EventArgs> OnOpen;
        public event Action<object, EventArgs> OnRename;

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
                    this.OnOpen?.Invoke(this, null);
                });
            }
        }


        public RelayCommand RenameCommand
        {
            get
            {
                return new RelayCommand((win) =>
                {
                    this.OnRename?.Invoke(this, null);
                });
            }
        }



    }
}
