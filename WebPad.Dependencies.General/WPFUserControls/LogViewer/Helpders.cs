using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace WebPad.Dependencies.General.WPFUserControls.LogViewer
{
    public static class Helpders
    {
        /// <summary>
        /// This should be called after logging has been initialized
        /// </summary>
        public static void ShowLog4NetWindow()
        {

            WPFUtilities.WindowHelpers.ShowWindowOutsideWPF((win) =>
            {
                var logViewer = new LogViewer.Log4NetViewerControl();
                win.Content = logViewer;
            });
        }
    }// end of Helpers class
}
