using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Models
{
    public class RecentHtmlSnippet : nac.ViewModelBase.ViewModelBase
    {

        public event Action<object, EventArgs> OnOpen;

        public string BaseFilePath
        {
            get { return GetValue(() => this.BaseFilePath); }
            set { SetValue(() => this.BaseFilePath, value); }
        }

        public string FilePath
        {
            get { return GetValue(() => this.FilePath); }
            set { SetValue(() => this.FilePath, value); }
        }


        public string FileName
        {
            get { return GetValue(() => this.FileName); }
            set { SetValue(() => this.FileName, value); }
        }


        public nac.wpf.utilities.RelayCommand OpenCommand
        {
            get
            {
                return new nac.wpf.utilities.RelayCommand((param) =>
                {
                    this.OnOpen?.Invoke(this, null);
                });
            }
        }
    }
}
