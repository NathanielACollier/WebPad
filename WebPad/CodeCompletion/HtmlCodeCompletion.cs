using System.Collections.Generic;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace WebPad.CodeCompletion
{
    internal class HtmlCodeCompletion : CodeCompletionBase
    {
        private const string ActivatedOn = "<";

        public HtmlCodeCompletion(TextEditor textEditor) : base(textEditor, ActivatedOn)
        {
        }

        protected override void AddCompletionData(IList<ICompletionData> data)
        {
            data.Add(new CodeCompletionDataBase(this, "<!-- -->", "A comment"));
            data.Add(new CodeCompletionDataBase(this, "<!DOCTYPE"));
            data.Add(new CodeCompletionDataBase(this, "a"));
            data.Add(new CodeCompletionDataBase(this, "abbr"));
            data.Add(new CodeCompletionDataBase(this, "acronym"));
            data.Add(new CodeCompletionDataBase(this, "address"));
            data.Add(new CodeCompletionDataBase(this, "area"));
            data.Add(new CodeCompletionDataBase(this, "b"));
            data.Add(new CodeCompletionDataBase(this, "base"));
            data.Add(new CodeCompletionDataBase(this, "bdo"));
            data.Add(new CodeCompletionDataBase(this, "big"));
            data.Add(new CodeCompletionDataBase(this, "blockquote"));
            data.Add(new CodeCompletionDataBase(this, "body"));
            data.Add(new CodeCompletionDataBase(this, "br"));
            data.Add(new CodeCompletionDataBase(this, "button"));
            data.Add(new CodeCompletionDataBase(this, "caption"));
        }
    }
}