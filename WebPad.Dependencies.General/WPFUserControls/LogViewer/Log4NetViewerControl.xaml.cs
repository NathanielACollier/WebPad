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
using System.Collections.ObjectModel;
using System.Threading; // assembly=System.Windows.Presentation

using System.Windows.Threading;
using OfficeOpenXml;
using System.Data;
using System.IO;

namespace WebPad.Dependencies.General.WPFUserControls.LogViewer
{
    /// <summary>
    /// Interaction logic for Log4NetViewerControl.xaml
    /// </summary>
    public partial class Log4NetViewerControl : UserControl
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ObservableCollection<Log4NetLogEntry> logEntries;

        /// <summary>
        /// This event will fire once the viewer is setup, and is able to display log entries from log4net, using the NotifyAppender
        /// </summary>
        public event EventHandler SetupComplete;

        public Log4NetViewerControl()
        {
            InitializeComponent();

            logEntries = new ObservableCollection<Log4NetLogEntry>();
            logViewerCtrl.DataContext = logEntries;

            StartWaitForLog4NetToBeConfiguredThread();
        }

        private void StartWaitForLog4NetToBeConfiguredThread()
        {
            Thread t = new Thread(() => {
                while (Log4NetHelpers.CodeConfiguredUtilities.IsLog4NetConfigured() == false)
                {
                    // Thread.Sleep(100); // just wait for a brief period

                }

                // got out of the loop, so it's configured now
                this.Dispatcher.Invoke(() => {
                    log.Debug("log4net is now configured, so setting up the notify appender");
                    SetupLogCapture(); // basicly transfer back to GUI Thread
                });
            });

            log.Debug("Starting the wait for log4net to be configured thread");
            t.Start();
        }


        private void SetupLogCapture()
        {
            Log4NetHelpers.CodeConfiguredUtilities.AddNotifyAppender((sender, args) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    var newEntry = new Log4NetLogEntry
                    {
                        DateTime = DateTime.Now,
                        Message = args.Message,
                        Level = args.Level,
                        LoggerName = args.LoggerName
                    };

                    logEntries.Insert(0, newEntry); // insert at top so we don't have to do fancy scrolling
                    //LogEntries.Add(newEntry);
                });

            });// end of add notify appender

            if (SetupComplete != null)
            {
                SetupComplete(this, new EventArgs());
            }
            else
            {
                log.Debug("Notify appender is now setup.  Log messages should start appearing.");
            }
        }

        private void ExportLogButton_Click(object sender, RoutedEventArgs e)
        {
            // convert to datatable and then use our functions to prompt the user for where to save, and then save it as excel
            var fileSaveDialog = new Microsoft.Win32.SaveFileDialog();
            fileSaveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
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
                    logTable,
                    printHeader: true,
                    worksheetName: "log"
                    );

                var clearQuestionMsg = System.Windows.MessageBox.Show("Do you want to clear log?", "", MessageBoxButton.YesNo);
                if (clearQuestionMsg == MessageBoxResult.Yes)
                {
                    ClearLog();
                }
            }
        }


        /// <summary>
        /// Original code from: http://stackoverflow.com/questions/13669733/export-datatable-to-excel-with-epplus
        /// </summary>
        /// <param name="filePath"></param>
        public static void SaveDataTableToExcel(string filePath, DataTable table, bool printHeader = false, string worksheetName = "data")
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var fs = new System.IO.FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Delete))
            {
                using (ExcelPackage pkg = new ExcelPackage(fs))
                {
                    ExcelWorksheet ws = pkg.Workbook.Worksheets[worksheetName];
                    if (ws == null)
                    {
                        ws = pkg.Workbook.Worksheets.Add(worksheetName);
                    }

                    ws.Cells["A1"].LoadFromDataTable(table, printHeader);

                    pkg.Save();
                }
            }
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
    }// end of class
}
