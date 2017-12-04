using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;

namespace WebPad.Rendering
{
    public class HtmlRenderer
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // built in internet explorer
        //  Will use whatever version the user has installed
        private readonly System.Windows.Controls.WebBrowser _myBrowser;

        

        public HtmlRenderer(System.Windows.Controls.Panel controlPanel )
        {
            // setup and host the internet explorer web browser that is built into the .net Windows Forms framework
            try
            {
                _myBrowser = new WebBrowser();

                bool navigateFiredOnce = false;

                _myBrowser.Navigated += (sender, args) =>
                {
                    // suppress script errors
                    if( navigateFiredOnce == false)
                    {
                        log.Info("Suppressing Browser Script Errors");
                        Utilities.WebBrowserUtil.HideScriptErrors(_myBrowser, true);
                        navigateFiredOnce = true;
                    }
                    
                };


                _myBrowser.LoadCompleted += (sender, args) =>
                {
                    // to be able to get clicks in the document
                    //  -- see: https://stackoverflow.com/questions/2189510/wpf-webbrowser-mouse-events-not-working-as-expected
                    //  -- and: https://msdn.microsoft.com/en-us/library/aa769764(v=vs.85).aspx
                    //  -- and: https://weblog.west-wind.com/posts/2004/Apr/27/Handling-mshtml-Document-Events-without-Mouse-lockups
                    var doc = _myBrowser.Document as MSHTML.HTMLDocument;

                    var doc2 = doc as MSHTML.DispHTMLDocument; // events are on this guy

                    var onClickHandler = new DHTMLEventHandler(doc);
                    onClickHandler.Handler += new DHTMLEvent(this.myWebBrowser_DocumentClickEvent);

                    doc2.onclick = onClickHandler;

                    var onMouseOverHandler = new DHTMLEventHandler(doc);
                    onMouseOverHandler.Handler += new DHTMLEvent(this.DocWithEvents_onmouseover);

                    doc2.onmouseover = onMouseOverHandler;

                    var onMouseOutHandler = new DHTMLEventHandler(doc);
                    onMouseOutHandler.Handler += new DHTMLEvent(this.DocWithEvents_onmouseout);

                    doc2.onmouseout = onMouseOutHandler;
                };




                controlPanel.Children.Add(_myBrowser);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Problem occured loading internet explorer browser.  Exception: {0}", ex), "Internet Explorer Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }



        // idea and original code for highlight on hover from: https://stackoverflow.com/questions/1090754/c-sharp-web-browser-with-click-and-highlight
        // keep up with what elements we've marked with a highlight
        //   - also holds the original style so we can set it back after the mouse leaves
        private IDictionary<MSHTML.IHTMLElement, string> elementStyles = new Dictionary<MSHTML.IHTMLElement, string>();

        private void DocWithEvents_onmouseover(MSHTML.IHTMLEventObj pEvtObj)
        {
            if(!this.elementStyles.ContainsKey(pEvtObj.srcElement))
            {
                // capture the original style so we can set it back when mouse leaves
                string style = pEvtObj.srcElement.style.cssText;
                this.elementStyles.Add(pEvtObj.srcElement, style); // saved
                // now change it
                pEvtObj.srcElement.style.cssText = style + "; background-color: purple;";
                
            }
        }


        private void DocWithEvents_onmouseout(MSHTML.IHTMLEventObj pEvtObj)
        {
            if(this.elementStyles.ContainsKey(pEvtObj.srcElement))
            {
                string originalStyle = this.elementStyles[pEvtObj.srcElement];
                this.elementStyles.Remove(pEvtObj.srcElement);
                pEvtObj.srcElement.style.cssText = originalStyle;
            }
        }



        protected void myWebBrowser_DocumentClickEvent(MSHTML.IHTMLEventObj args)
        {
            if( args.srcElement != null)
            {
                var lineNumber = args.srcElement.getAttribute("linenumber", 0) as string;
                var column = args.srcElement.getAttribute("column", 0) as string;

                if( lineNumber != null)
                {
                    log.Info($"Element clicked with AvalonEdit html source line number {lineNumber}");
                }
            }

            // see: https://weblog.west-wind.com/posts/2004/Apr/27/Handling-mshtml-Document-Events-without-Mouse-lockups
            args.returnValue = false;
        }


        public void ScrollBrowserTo(int lineNumber, int column)
        {
            if( _myBrowser != null)
            {
                var doc = _myBrowser.Document as MSHTML.HTMLDocument;
                // if the user hasn't hit F5 then the web browser control won't show anything
                if ( doc != null)
                {
                    var lineAnchors = doc.getElementsByClassName(Rendering.HtmlInjectLineNumberAnchors.cssClassMarker)
                                                .OfType<MSHTML.HTMLGenericElement>();

                    if(lineAnchors.Any())
                    {
                        // first element greater than or equal to line number (Scroll to it)
                        var greaterThanEqualToLineNumber = from e in lineAnchors
                                                           let num = Convert.ToInt32(e.getAttribute("linenumber") as string)
                                                           where num >= lineNumber
                                                           select e;

                        if (greaterThanEqualToLineNumber.Any())
                        {
                            var target = greaterThanEqualToLineNumber.First();
                            log.Info($"Scrolling to element with source line {target.getAttribute("linenumber", 0)}, column {target.getAttribute("column")}");
                            target.scrollIntoView();
                        }
                        else
                        {
                            log.Info($"No element found at line {lineNumber}, so scrolling to end");
                            lineAnchors.Last().scrollIntoView();
                        }
                    }// end of if any line anchors

                    
                 }// end of if doc != null
                
            }
        }




        public void Render( UserControls.SnippetDocumentControl snippetDocControl)
        {
            // load the document text into the internet explorer browser
            if (_myBrowser != null)
            {
                try
                {
                    string url = snippetDocControl.EnsureServer();
                    _myBrowser.Navigate(url);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(string.Format("Problem with IE browser loading html.  Exception: {0}", ex), "IE HTML Load Exception", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

        }







    }
}