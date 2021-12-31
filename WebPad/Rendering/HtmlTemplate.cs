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

        private static readonly string _javascriptStartupcode =
            ResourceUtilities.GetEmbeddedResourceString("WebPad.Resources.startupCode.js");

        public static string GetDocumentText(UserControls.SnippetDocumentControl snippetDocControl)
        {
            string baseHref = snippetDocControl.BaseHref;
            if(!string.IsNullOrWhiteSpace(snippetDocControl.SaveFilePath))
            {
                baseHref = "/"; // root, because we are going to dynamically determine where the files should come from
            }

            return _basePageHtml.AsTemplated(new
            {
                //References = snippetDocControl.References.GetHtml(),
                jsRefs = References.GetHtml(snippetDocControl.References.Where(i => i.Type == ReferenceTypes.Javascript)
                ),
                cssRefs = References.GetHtml(snippetDocControl.References.Where(i => i.Type == ReferenceTypes.Css)
                    ),
                Javascript = snippetDocControl.Javascript,
                Html = HtmlInjectLineNumberAnchors.Inject(snippetDocControl.Html),
                CSS = snippetDocControl.CSS,
                PageTitle = snippetDocControl.SaveFileName,
                BaseHref = baseHref,
                javascriptStartupCode = _javascriptStartupcode
            });
        }
    }
}
