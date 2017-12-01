using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Dependencies.General.Utilities
{
    public static class WPFWindowsFormsIntegrationUtil
    {
        /*
          This code was taken from a stackoverflow post.  It may not be around anymore.  It was origionally at http://stackoverflow.com/questions/315164/how-to-use-a-folderbrowserdialog-from-a-wpf-application

          The other part that went with this code was to use it by doing the following.  I have tested this code and it works in the nacBackupWPF project

          var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
          System.Windows.Forms.DialogResult result = folderDialog.ShowDialog(MyWpfExtensions.GetIWin32Window(this));

         */

        public static System.Windows.Forms.IWin32Window GetIWin32Window(System.Windows.Media.Visual visual)
        {
            var source = System.Windows.PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            private readonly System.IntPtr _handle;
            public OldWindow(System.IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members
            System.IntPtr System.Windows.Forms.IWin32Window.Handle
            {
                get { return _handle; }
            }
            #endregion
        }
    }
}
