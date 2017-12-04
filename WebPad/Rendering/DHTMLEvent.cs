using MSHTML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Rendering
{
    // code from here: https://weblog.west-wind.com/posts/2004/Apr/27/Handling-mshtml-Document-Events-without-Mouse-lockups
    ///
    /// Generic HTML DOM Event method handler.
    ///
    public delegate void DHTMLEvent(IHTMLEventObj e);

    ///
    /// Generic Event handler for HTML DOM objects.
    /// Handles a basic event object which receives an IHTMLEventObj which
    /// applies to all document events raised.
    ///
    public class DHTMLEventHandler
    {
        public DHTMLEvent Handler;
        HTMLDocument Document;

        public DHTMLEventHandler(HTMLDocument doc)
        {
            this.Document = doc;
        }
        [DispId(0)]
        public void Call()
        {
            Handler(Document.parentWindow.@event);
        }
    }
}
