using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Controls;

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
                    var docWithEvents = _myBrowser.Document as MSHTML.HTMLDocumentEvents2_Event;

                    docWithEvents.onclick += new MSHTML.HTMLDocumentEvents2_onclickEventHandler(this.myWebBrowser_DocumentClickEvent);
                };




                controlPanel.Children.Add(_myBrowser);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Problem occured loading internet explorer browser.  Exception: {0}", ex), "Internet Explorer Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }



        protected bool myWebBrowser_DocumentClickEvent(MSHTML.IHTMLEventObj args)
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

            return false;
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