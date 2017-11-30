using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace WebPad.CodeCompletion
{
    internal class CodeCompletionDataBase : ICompletionData
    {
        protected readonly CodeCompletionBase CodeCompletion;

        public CodeCompletionDataBase(CodeCompletionBase codeCompletion, string text)
        {
            CodeCompletion = codeCompletion;
            Text = text;
        }

        public CodeCompletionDataBase(CodeCompletionBase codeCompletion, string text, string description)
        {
            CodeCompletion = codeCompletion;
            Text = text;
            Description = description;
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public virtual ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        public virtual object Description { get; private set; }

        public virtual object Content
        {
            get { return Text; }
        }

        public double Priority
        {
            get { return 0; }
        }
    }
}