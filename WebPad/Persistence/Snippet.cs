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
            this.Html = snippetControl.Html;
            this.Javascript = snippetControl.Javascript;
            this.References = snippetControl.References;
        }

        public string Html { get; set; }
        public string Javascript { get; set; }
        public string CSS { get; set; }
        public Rendering.References References { get; set; }
    }
}