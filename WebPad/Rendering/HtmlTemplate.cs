using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebPad.Utilities;

namespace WebPad.Rendering
{
    public class HtmlTemplate
    {
        private static readonly string _basePageHtml = ResourceUtilities.GetEmbeddedResourceString("WebPad.Resources.Templates.ResultsPage.html");


        public static string GetDocumentText(UserControls.SnippetDocumentControl snippetDocControl)
        {
            return _basePageHtml.AsTemplated(new
            {
                //References = snippetDocControl.References.GetHtml(),
                jsRefs = References.GetHtml(snippetDocControl.References.Where(i => i.Type == ReferenceTypes.Javascript)
                ),
                cssRefs = References.GetHtml(snippetDocControl.References.Where(i => i.Type == ReferenceTypes.Css)
                    ),
                Javascript = snippetDocControl.Javascript,
                Html = snippetDocControl.Html,
                CSS = snippetDocControl.CSS,
                PageTitle = snippetDocControl.SaveFileName,
                BaseHref = string.IsNullOrWhiteSpace(snippetDocControl.BaseHref) ? "/" : snippetDocControl.BaseHref
            });
        }
    }
}
