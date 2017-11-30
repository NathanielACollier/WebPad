using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using WebPad.CodeCompletion.jQueryAPI;
using WebPad.Utilities;

namespace WebPad.CodeCompletion
{
    class JQueryCodeCompletion : CodeCompletionBase
    {
        private const string ActivationCharacters = @".""";
        private readonly IList<CodeCompletionDataBase> _memberCompletionData = new List<CodeCompletionDataBase>();
        private readonly IList<CodeCompletionDataBase> _selectorCompletionData = new List<CodeCompletionDataBase>();

        public JQueryCodeCompletion(TextEditor textEditor) : base(textEditor, ActivationCharacters)
        {
            ExtractIntellisenseFromXml();
        }

        private void ExtractIntellisenseFromXml()
        {
            var loader = new jQueryAPI.Loader();
            var docs = loader.LoadFromResource();

            docs.Entries
                .Where(entry => entry.Type == MemberType.Method || entry.Type == MemberType.Property)
                .Select(entry => new JQueryCodeCompletionData(this, entry))
                .Apply(cc => _memberCompletionData.Add(cc))
                .ToList();

            docs.Entries
                .Where(entry => entry.Type == MemberType.Selector)
                .Select(entry => new JQueryCodeCompletionData(this, entry))
                .Apply(cc => _selectorCompletionData.Add(cc))
                .ToList();
        }

        protected override void AddCompletionData(IList<ICompletionData> data)
        {
            data.Clear();
            if (ActivatedOn == ".")
                _memberCompletionData.Apply(data.Add).ToList();
            if (ActivatedOn == @"""")
                _selectorCompletionData.Apply(data.Add).ToList();
        }
    }
}