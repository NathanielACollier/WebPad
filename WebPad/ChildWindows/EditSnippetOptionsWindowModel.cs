using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.ChildWindows
{
    public class EditSnippetOptionsWindowModel : nac.ViewModelBase.ViewModelBase
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


        public ObservableCollection<Models.RecentHtmlSnippet> RecentSnippets
        {
            get { return GetValue(() => this.RecentSnippets); }
        }

    }
}
