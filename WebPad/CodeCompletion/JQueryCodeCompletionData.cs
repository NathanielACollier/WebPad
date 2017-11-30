using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WebPad.CodeCompletion.jQueryAPI;
using WebPad.Utilities;

namespace WebPad.CodeCompletion
{
    internal class JQueryCodeCompletionData : CodeCompletionDataBase
    {
        private readonly Entry _entry;

        static readonly ImageSource IconMethod = BitmapUtilities.CreateTransparentBitmap("pack://application:,,,/Resources/Images/Intellisense/VSObject_Method.bmp");
        static readonly ImageSource IconProperty = BitmapUtilities.CreateTransparentBitmap("pack://application:,,,/Resources/Images/Intellisense/VSObject_Properties.bmp");
        static readonly ImageSource IconSelector = BitmapUtilities.CreateTransparentBitmap("pack://application:,,,/Resources/Images/Intellisense/VSObject_Macro.bmp");
        static readonly ImageSource IconTemplate = BitmapUtilities.CreateTransparentBitmap("pack://application:,,,/Resources/Images/Intellisense/Control_Form.bmp");

        public JQueryCodeCompletionData(CodeCompletionBase codeCompletion, Entry entry) : base(codeCompletion, entry.Name)
        {
            _entry = entry;
        }

        public override object Content
        {
            get
            {
                return Text;
            }
        }

        public override ImageSource Image
        {
            get
            {
                switch (Entry.Type)
                {
                    case MemberType.Method:
                        return IconMethod;
                    case MemberType.Property:
                        return IconProperty;
                    case MemberType.Selector:
                        return IconSelector;
                    case MemberType.TemplateTag:
                        return IconTemplate;
                    default:
                        return IconMethod;
                }
            }
        }

        public override object Description
        {
            get { return RenderDescription(); }
        }

        public Entry Entry
        {
            get { return _entry; }
        }

        /// <summary>
        /// Formats the intellisence popup description with the colour syntax of the current entry.
        /// </summary>
        /// <returns></returns>
        private object RenderDescription()
        {
            var rowNumber = 0;

            var grid = new Grid();

            foreach (var signature in Entry.Signatures)
            {
                var member = new TextBlock();
                grid.RowDefinitions.Add(new RowDefinition());
                Grid.SetRow(member, rowNumber++);

                if (Entry.Type == MemberType.TemplateTag)
                {
                    // Templates don't have all the extra member syntax to render
                    member.Inlines.Add(new Run(Text) {Foreground = Brushes.Black, FontWeight = FontWeights.Bold});
                }
                else
                {
                    if (Entry.Type == MemberType.Method || Entry.Type == MemberType.Property)
                    {
                        member.Inlines.Add(new Run(Entry.ReturnType + " ") {Foreground = Brushes.Green});
                    }
                    member.Inlines.Add(new Run(Text) {Foreground = Brushes.Black, FontWeight = FontWeights.Bold});
                    member.Inlines.Add(new Run("(") {Foreground = Brushes.Black});

                    var renderedFirst = false;
                    foreach (var argument in signature.Arguments)
                    {
                        if (renderedFirst) member.Inlines.Add(new Run(", "));
                        renderedFirst = true;

                        if (argument.Optional) member.Inlines.Add(new Run("["));
                        if (argument.Type.ToLower() == "function")
                            member.Inlines.Add(new Run("function ") {Foreground = Brushes.Green});
                        member.Inlines.Add(new Run(argument.Name));
                        if (argument.Optional) member.Inlines.Add(new Run("]"));
                    }
                    member.Inlines.Add(new Run(")") {Foreground = Brushes.Black});
                }
                grid.Children.Add(member);

                foreach (var argument in signature.Arguments)
                {
                    member = new TextBlock();
                    grid.RowDefinitions.Add(new RowDefinition());
                    Grid.SetRow(member, rowNumber++);
                    member.Inlines.Add(new Run(argument.Name + ": ") {FontWeight = FontWeights.Bold});
                    member.Inlines.Add(new Run(argument.Description));
                    grid.Children.Add(member);
                }
            }

            grid.RowDefinitions.Add(new RowDefinition());
            grid.Children.Add(CreateTextBlock(Entry.Description, Brushes.Green, FontWeights.Normal, rowNumber++));

            return grid;
        }

        private static TextBlock CreateTextBlock(string text, Brush color, FontWeight fontWeight, int row)
        {
            var textBlock = new TextBlock {Text = text, Foreground = color, FontWeight = fontWeight };
            Grid.SetRow(textBlock, row);
            return textBlock;
        }
    }
}