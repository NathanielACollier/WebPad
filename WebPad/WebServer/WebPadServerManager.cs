using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace WebPad.WebServer
{
    public class WebPadServerManager
    {

        private UserControls.SnippetDocumentControl control;

        private string _url;
        public string Url { get { return _url; }  }
        private WebServer server;

        public bool IsRunning { get; set; }

        public WebPadServerManager(UserControls.SnippetDocumentControl _ctrl)
        {
            this.control = _ctrl;
        }



        public void Start()
        {
            if( this.IsRunning)
            {
                throw new Exception("Allready running");
            }
            int retryLimit = 5;

            do
            {

                try
                {
                    int port = TCPUtillity.FreeTcpPort();
                    this._url = $"http://localhost:{port}/";

                    this.server = new WebServer((request)=>{
                        string text = "";
                        // see: http://stackoverflow.com/questions/23442543/using-async-await-with-dispatcher-begininvoke
                        this.control.Dispatcher.InvokeAsync(() =>
                        {
                            text = Rendering.HtmlTemplate.GetDocumentText(this.control);
                        }).Wait();

                        return text;
                    }, this.Url);
                    this.server.Run();
                    this.IsRunning = true;
                }
                catch( Exception ex)
                {

                }
                Thread.Sleep(100);
                --retryLimit;
            } while ( !this.IsRunning && retryLimit > 0);
        }

        public void Stop()
        {
            this.server.Stop();
            this.IsRunning = false;
        }
    }
}
