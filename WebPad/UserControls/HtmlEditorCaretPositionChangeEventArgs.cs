using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.UserControls
{
    public class HtmlEditorCaretPositionChangeEventArgs : EventArgs
    {
        public int LineNumber { get; set; }
        public int Column { get; set; } // position in the line where the caret is
    }
}
