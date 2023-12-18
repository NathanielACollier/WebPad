using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Dependencies.General.WPFUserControls.LocalFolderBrowser
{
    public class FileSystemNodeModel : nac.ViewModelBase.ViewModelBase
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
    }
}
