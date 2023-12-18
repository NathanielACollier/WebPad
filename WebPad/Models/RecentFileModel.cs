using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WebPad.Models
{
    public class RecentFileModel : nac.ViewModelBase.ViewModelBase
    {
        public event Action<object, EventArgs> OnOpen;
        public event Action<object, EventArgs> OnRename;

        public event Action<object, EventArgs> OnRemove;
        public event Action<object, EventArgs> OnClearAll;

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

        public ICommand OpenRecentFileCommand
        {
            get {
                return new nac.wpf.utilities.RelayCommand(win =>
                {
                    this.OnOpen?.Invoke(this, null);
                });
            }
        }


        public nac.wpf.utilities.RelayCommand RenameCommand
        {
            get
            {
                return new nac.wpf.utilities.RelayCommand((win) =>
                {
                    this.OnRename?.Invoke(this, null);
                });
            }
        }

        public nac.wpf.utilities.RelayCommand RemoveCommand
        {
            get
            {
                return new nac.wpf.utilities.RelayCommand((win) =>
                {
                    this.OnRemove?.Invoke(this, null);
                });
            }
        }


        public nac.wpf.utilities.RelayCommand ClearAllCommand
        {
            get
            {
                return new nac.wpf.utilities.RelayCommand((win) =>
                {
                    this.OnClearAll?.Invoke(this,null);
                });
            }
        }


    }
}
