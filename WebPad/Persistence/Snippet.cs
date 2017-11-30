using WebPad.UserControls;
namespace WebPad.Persistence
{
    public class Snippet
    {
        public Snippet()
        {
        }
        
        public Snippet(string css, string html, string javascript, Rendering.References references)
        {
            CSS = css;
            Html = html;
            Javascript = javascript;
            References = references;
        }

        public Snippet(SnippetDocumentControl snippetControl)
        {
            this.CSS = snippetControl.CSS;

            if(string.IsNullOrWhiteSpace(snippetControl.ExternalHtmlPath))
            {
                this.Html = snippetControl.Html;
            }else
            {
                this.Html = ""; // clear it out if it somehow had something
                this.externalHtmlFile = snippetControl.ExternalHtmlPath;
            }
            
            this.Javascript = snippetControl.Javascript;
            this.References = snippetControl.References;

            this.baseHref = snippetControl.BaseHref;
        }

        public string Html { get; set; }
        public string Javascript { get; set; }
        public string CSS { get; set; }
        public Rendering.References References { get; set; }


        public string externalHtmlFile { get; set; } // this is to be able to edit angularjs component templates that are just html fragments
        public string baseHref { get; set; } // this is like <head><base href="" /></head>
    }
}