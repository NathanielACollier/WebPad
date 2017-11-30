using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WebPad.Rendering
{
    public class References : ObservableCollection<Reference>
    {
        public References()
        {
            
        }

        public void AddDefaults()
        {
            Add(new Reference(ReferenceTypes.Javascript, "http://ajax.googleapis.com/ajax/libs/jquery/1.6.4/jquery.min.js"));
            Add(new Reference(ReferenceTypes.Javascript, "http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.16/jquery-ui.min.js"));
        }

        public string GetHtml()
        {
            return GetHtml(this.AsEnumerable());
        }

        public static string GetHtml(IEnumerable<Reference> refs)
        {
            return refs.Aggregate(string.Empty, (current, reference) => current + (reference.GetHtml() + Environment.NewLine));
        }
    }
}