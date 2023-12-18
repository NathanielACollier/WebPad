using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
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

        private static nac.Logging.Logger log = new();
        private ObservableCollection<LogEntry> logEntries;

        /// <summary>
        /// This event will fire once the viewer is setup, and is able to display log entries
        /// </summary>
        public event EventHandler SetupComplete;

        /// <summary>
        /// Original from: http://stackoverflow.com/questions/16743804/implementing-a-log-viewer-with-wpf
        /// Turned it into a control, instead of a basic window and did some other things with it...
        /// </summary>
        public LogViewerControl()
        {
            InitializeComponent();

            logEntries = new ObservableCollection<LogEntry>();
            logViewerCtrl.DataContext = logEntries;

            nac.Logging.Logger.OnNewMessage += (_s, args) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    var newEntry = new LogEntry
                    {
                        DateTime = DateTime.Now,
                        Message = args.Message,
                        Level = args.Level,
                        LoggerName = $"{args.CallingClassType.FullName}.{args.CallingMemberName}"
                    };

                    logEntries.Insert(0, newEntry); // insert at top so we don't have to do fancy scrolling
                    //LogEntries.Add(newEntry);
                });
            };
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



        private void ExportLogButton_Click(object sender, RoutedEventArgs e)
        {
            // convert to datatable and then use our functions to prompt the user for where to save, and then save it as excel
            var fileSaveDialog = new Microsoft.Win32.SaveFileDialog();
            fileSaveDialog.Filter = "CSV Files (*.csv)|*.csv";
            fileSaveDialog.FileName = "Log";

            var logTable = new System.Data.DataTable();
            logTable.Columns.AddRange((new[] { "Index", "Date", "Level", "LoggerName", "Message" })
                                        .Select(name => new System.Data.DataColumn(name))
                                        .ToArray()
                                        );
            foreach (var entry in this.logEntries)
            {
                var row = logTable.NewRow();
                logTable.Rows.Add(row);
                row["Index"] = entry.Index;
                row["Date"] = $"{entry.DateTime:yyyy-MM-dd hh:mm:ss tt}";
                row["Level"] = entry.Level;
                row["LoggerName"] = entry.LoggerName;
                row["Message"] = entry.Message;
            }

            if (fileSaveDialog.ShowDialog() == true)
            {
                SaveDataTableToExcel(
                    fileSaveDialog.FileName,
                    logTable
                    );

                var clearQuestionMsg = System.Windows.MessageBox.Show("Do you want to clear log?", "", MessageBoxButton.YesNo);
                if (clearQuestionMsg == MessageBoxResult.Yes)
                {
                    ClearLog();
                }
            }
        }


        private static void SaveDataTableToExcel(string filePath, DataTable table)
        {
            
            using (var fs = new System.IO.FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Delete))
            {
                using (var fout = new System.IO.StreamWriter(fs))
                {
                    // build header
                    fout.WriteLine(
                        BuildLine(
                            table.Columns.OfType<DataColumn>().Select(c => c.ColumnName).ToArray()
                            )
                        );

                    // build rows
                    foreach( var row in table.AsEnumerable())
                    {
                        fout.WriteLine(
                            BuildLine(
                                row.ItemArray.Select(i => i?.ToString() ?? "").ToArray()
                                )
                            );
                    }
                }
            }
        }


        private static string BuildLine(params string[] CsvColumns)
        {
            StringBuilder sb = new StringBuilder();
            string prefix = "";
            foreach (string temp in CsvColumns)
            {
                if (temp.Contains('"') || temp.Contains(','))
                {
                    sb.Append(prefix);
                    sb.Append("\"");
                    sb.Append(temp.Replace("\"", "\"\""));
                    sb.Append("\"");
                }
                else
                {
                    sb.Append(prefix);
                    sb.Append(temp);
                }
                prefix = ",";
            }
            return sb.ToString();
        }


        private void ClearLog()
        {
            this.logEntries.Clear();
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to clear the log?", "", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                ClearLog();
            }
        }


    }
}
