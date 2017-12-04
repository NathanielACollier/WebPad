using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Rendering
{
    public class WebBrowserElementClickedEventArgs : EventArgs
    {
        public string LineNumber { get; set; }
        public string Column { get; set; }
    }
}
