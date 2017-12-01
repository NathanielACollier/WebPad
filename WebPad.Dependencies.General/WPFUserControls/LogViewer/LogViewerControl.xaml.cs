using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebPad.Dependencies.General.WPFUserControls.LogViewer
{

    /// <summary>
    /// Interaction logic for LogViewerControl.xaml
    /// </summary>
    public partial class LogViewerControl : UserControl
    {
        /// <summary>
        /// Original from: http://stackoverflow.com/questions/16743804/implementing-a-log-viewer-with-wpf
        /// Turned it into a control, instead of a basic window and did some other things with it...
        /// </summary>
        public LogViewerControl()
        {
            InitializeComponent();
        }


        private void LogMessageTextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            var logEntry = (sender as TextBlock).DataContext as LogEntry;
            logEntry.IsMessageCopyable = true; // mouse is over textblock so user should be able to copy the message
        }

        private void LogMessageTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            var logEntry = (sender as TextBox).DataContext as LogEntry;
            logEntry.IsMessageCopyable = false; // mouse left so we want the textbox to go away
        }






    }
}
