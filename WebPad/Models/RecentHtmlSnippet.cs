using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPad.Dependencies.General.WPFViewModelBase;

namespace WebPad.Models
{
    public class RecentHtmlSnippet : ViewModelBase
    {

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


        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand((param) =>
                {
                    // fire off event that open happened
                });
            }
        }
    }
}
