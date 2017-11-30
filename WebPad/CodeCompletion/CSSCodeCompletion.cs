using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace WebPad.CodeCompletion
{
    internal class CSSCodeCompletion : CodeCompletionBase
    {
        private const string ActivatedOn = " ";

        public CSSCodeCompletion(TextEditor textEditor) : base(textEditor, ActivatedOn)
        {
        }

        //todo:  This needs to be replaced with a pull from an xml file.  I think you can use excel to pull web data from this page: http://www.w3.org/TR/CSS21/propidx.html.  Then get that converted to an xml file that could be read in and loaded as code complete data
        protected override void AddCompletionData(IList<ICompletionData> data)
        {
            data.Add(new CodeCompletionDataBase(this, "background-color", "<color> | transparent | inherit"));
            data.Add(new CodeCompletionDataBase(this, "font-size", "<absolute-size> | <relative-size> | <length> | <percentage> | inherit"));
        }

    }
}
