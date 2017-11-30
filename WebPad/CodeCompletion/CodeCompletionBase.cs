using System;
using System.Collections.Generic;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using WebPad.Utilities;

namespace WebPad.CodeCompletion
{
    internal abstract class CodeCompletionBase
    {
        private readonly TextEditor _textEditor;
        private readonly string _activatedOn;
        private CompletionWindow _completionWindow;

        protected CodeCompletionBase(TextEditor textEditor, string activatedOn)
        {
            _textEditor = textEditor;
            _activatedOn = activatedOn;
            _textEditor.TextArea.TextEntering += OnTextEntering;
            _textEditor.TextArea.TextEntered += OnTextEntered;
        }

        /// <summary>
        /// That key press that caused the intellisence to be displayed
        /// </summary>
        protected string ActivatedOn { get; set; }

        protected abstract void AddCompletionData(IList<ICompletionData> data);

        void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.NotContainedWithin(_activatedOn)) 
                return;

            ActivatedOn = e.Text;
            _completionWindow = new CompletionWindow(_textEditor.TextArea);
            var data = _completionWindow.CompletionList.CompletionData;

            AddCompletionData(data);
            _completionWindow.Show();
            _completionWindow.Closed += (obj, args) => _completionWindow = null;
        }

        void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length <= 0 || _completionWindow == null) return;
            if (char.IsLetterOrDigit(e.Text[0])) return;
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }
}