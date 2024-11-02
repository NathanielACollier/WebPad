using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using nac.WebServer.lib;

namespace WebPad.WebServer
{
    public class WebPadServerManager
    {
        private static nac.Logging.Logger log = new();

        private nac.WebServer.WebServerManager webManager = new();

        private UserControls.SnippetDocumentControl control;

        public bool IsRunning
        {
            get { return webManager.IsRunning; }
        }

        public string Url
        {
            get { return webManager.Url; }
        }

        public WebPadServerManager(UserControls.SnippetDocumentControl _ctrl)
        {
            this.control = _ctrl;
            this.webManager.OnNewRequest += WebManager_OnNewRequest;

            nac.WebServer.lib.Log.OnNewMessage += (_s, _args) =>
            {
                log.Info("[" + _args.CallerType + "." + _args.CallingMemberName + "]" + _args.Level + " - " + _args.Message);
            };
        }

        private void WebManager_OnNewRequest(object sender, nac.WebServer.WebServerManager.OnNewRequestEventArgs e)
        {
            if( e.Request.Url.LocalPath == "/")
            {
                serveRootHtml(e.Response);
                return;
            }

            serveStaticFile(e.Request, e.Response);
        }

        private void serveRootHtml(HttpListenerResponse response)
        {
            // serve webpage
            string text = "";
            // see: http://stackoverflow.com/questions/23442543/using-async-await-with-dispatcher-begininvoke
            this.control.Dispatcher.InvokeAsync(() =>
            {
                text = Rendering.HtmlTemplate.GetDocumentText(this.control);
            }).Wait();

            response.WriteTextResponse("text/html", text);
        }

        private void serveStaticFile(HttpListenerRequest request, HttpListenerResponse response)
        {
            // try to serve static file
            log.Info($"Incoming request: {request.Url.LocalPath}");
            string baseDirectory = null;

            if (!string.IsNullOrWhiteSpace(this.control.SaveFilePath))
            {
                baseDirectory = System.IO.Path.GetDirectoryName(this.control.SaveFilePath) + "\\";
                if (!string.IsNullOrWhiteSpace(this.control.BaseHref))
                {
                    baseDirectory = Utilities.PathUtilities.MakeAbsolutePath(baseDirectory, this.control.BaseHref);
                }
                log.Info($"Base Directory: {baseDirectory}");
                // serve the image???
                string filePath = baseDirectory + request.Url.LocalPath.Replace('/', '\\')
                                                            .Replace("~", "\\");
                log.Info($"Serving static file with [webpath={request.Url.LocalPath}, sysPath={filePath}]");

                if (System.IO.File.Exists(filePath))
                {
                    string ext = System.IO.Path.GetExtension(filePath);
                    var fileInfo = nac.WebServer.lib.HttpHelper.getFileServeInfo(ext);

                    if (!string.IsNullOrWhiteSpace(fileInfo.contentType))
                    {
                        if (fileInfo.IsBinary == false)
                        {
                            response.WriteTextResponse( fileInfo.contentType, System.IO.File.ReadAllText(filePath));
                        }
                        else
                        {
                            response.WriteBinaryResponse( fileInfo.contentType, System.IO.File.ReadAllBytes(filePath));
                        }
                    }
                    else
                    {
                        // binary octet stream
                        response.WriteBinaryResponse( "application/octet-stream", System.IO.File.ReadAllBytes(filePath));
                    }
                }
                else
                {
                    log.Warn($"File Not Found: {filePath}");
                    response.WriteTextResponse( "application/text", $"ERROR - No File Found at url [{request.Url.LocalPath}]");
                }


            }
        }

        public void Start()
        {
            this.webManager.Start();


        }

        public void Stop()
        {
            this.webManager.Stop();
        }
    }
}
