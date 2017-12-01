using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Dependencies.General.WPFUserControls.LocalFolderBrowser
{
    public class FileSelectedEventArgs : EventArgs
    {

        public FileModel SelectedFile { get; set; }

        public FileSelectedEventArgs()
        {

        }
    }
}
