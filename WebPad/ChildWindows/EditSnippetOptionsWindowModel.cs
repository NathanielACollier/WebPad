using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.ChildWindows
{
    public class EditSnippetOptionsWindowModel : WPFViewModelBase.ViewModelBase
    {

        public string ExternalHtmlTemplatePath
        {
            get { return this.GetValue(() => this.ExternalHtmlTemplatePath); }
            set { this.SetValue(() => this.ExternalHtmlTemplatePath, value); }
        }


        public string BaseHref
        {
            get { return this.GetValue(() => this.BaseHref); }
            set { this.SetValue(() => this.BaseHref, value); }
        }

    }
}
