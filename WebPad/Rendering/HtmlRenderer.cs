using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace WebPad.Rendering
{
    public class HtmlRenderer
    {
        private static nac.Logging.Logger log = new();

        public event Action<object, WebBrowserElementClickedEventArgs> WebBrowserElementClicked;

        // built in internet explorer
        //  Will use whatever version the user has installed
        private readonly Microsoft.Web.WebView2.Wpf.WebView2 _myBrowser;

        

        public HtmlRenderer(System.Windows.Controls.Panel controlPanel )
        {
            // setup and host the internet explorer web browser that is built into the .net Windows Forms framework
            try
            {
                _myBrowser = new Microsoft.Web.WebView2.Wpf.WebView2();

                bool navigateFiredOnce = false;

                _myBrowser.NavigationStarting += (sender, args) =>
                {
                    // suppress script errors
                    if( navigateFiredOnce == false)
                    {
                        // do whatever you might want to do only one time, in the entire browser navigation cycle
                        // Used to do the suppress script errors of IE here
                        // TODO: Probably remove this whole code block later
                        navigateFiredOnce = true;
                    }
                    
                };


                _myBrowser.NavigationCompleted += async (sender, args) =>
                {
                    // TODO: Need to setup document click stuff
                    // We lost all the IE document selection stuff, so would need to do that with javascript and events and stuff
                };

                controlPanel.Children.Add(_myBrowser);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Problem occured loading browser.  Exception: {ex}", "Browser Load Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }

        private async Task SetupDocumentInteraction()
        {
            // This should be called once navigation is completed, once we are at the final web page we can workout document events
            
            _myBrowser.CoreWebView2.WebMessageReceived += (_s, args) =>
            {
                handleWebView2_WebMessageReceived(jsonResult: args.WebMessageAsJson);
            };

            _myBrowser.CoreWebView2.DOMContentLoaded += async (_s, args) =>
            {
                await fireOffJavascriptDocumentsetupCode();
            };
            
        }

        private async Task<bool> fireOffJavascriptDocumentsetupCode()
        {
            string resultJSONText = await _myBrowser.CoreWebView2.ExecuteScriptAsync(@"
                ( () => {
                    return {
                        success: true
                    }
                })();
            ");
            
            // look at the result
            var result = System.Text.Json.Nodes.JsonNode.Parse(resultJSONText);

            if ((bool)result["success"] == true)
            {
                return true;
            }

            return false;
        }

        private void handleWebView2_WebMessageReceived(string jsonResult)
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<Models.HtmlTagInjectedAttributes>(jsonResult);

            if (string.Equals((string)result.type, "elementClick", StringComparison.OrdinalIgnoreCase))
            {
                log.Info($"Element clicked: [Line: {result.lineNumber}; Column: {result.column}] ");
                this.WebBrowserElementClicked?.Invoke(this._myBrowser, new WebBrowserElementClickedEventArgs
                {
                    LineNumber = result.lineNumber,
                    Column = result.column
                });
            }
        }


        public void ScrollBrowserTo(int lineNumber, int column)
        {
            if( _myBrowser?.CoreWebView2 != null)
            {
                _myBrowser.CoreWebView2.ExecuteScriptAsync($@"
                    // finding element by attribute see: https://stackoverflow.com/questions/2694640/find-an-element-in-dom-based-on-an-attribute-value
                    let target = document.querySelector('[linNumber=""{lineNumber}""] [column=""{column}""]');

                    target.scrollIntoView(); // see: https://developer.mozilla.org/en-US/docs/Web/API/Element/scrollIntoView                   
                ").ContinueWith(t =>
                {
                    log.Info($"Scrolling should be columented for Element[Line: {lineNumber}; column: {column}]");
                });

            }
        }




        public async Task Render( UserControls.SnippetDocumentControl snippetDocControl)
        {
            // load the document text into the internet explorer browser
            if (_myBrowser != null)
            {
                try
                {
                    // see info on how to navigate here: https://stackoverflow.com/questions/63116740/why-my-corewebview2-which-is-object-of-webview2-is-null
                    await _myBrowser.EnsureCoreWebView2Async();
                    await SetupDocumentInteraction();
                    string url = snippetDocControl.EnsureServer();
                    _myBrowser.CoreWebView2.Navigate(url);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(string.Format("Problem with IE browser loading html.  Exception: {0}", ex), "IE HTML Load Exception", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

        }







    }
}