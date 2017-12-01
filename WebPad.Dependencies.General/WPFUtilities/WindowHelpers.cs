using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WebPad.Dependencies.General.WPFUtilities
{
    public static class WindowHelpers
    {

        /// <summary>
        /// GUI Items have to be created on the GUI thread, so if we want to display a control or something like that on this window we need to use a callback delegate to change the window in the thread that created it
        /// </summary>
        /// <param name="callBack"></param>
        public static void ShowDialogWindowOutsideWPF(Action<System.Windows.Window> callBack)
        {
            // see this for how the setup was done: http://stackoverflow.com/questions/13381967/show-wpf-window-from-test-unit
            //  - The example actually describes using this scenario in a unit test
            var t = new Thread(() =>
            {
                // we have to do a little bit, so we can't just call the other function
                var win = new System.Windows.Window();

                // need to initiate proper shutdown when the window closes
                win.Closed += (sender, e) =>
                {
                    win.Dispatcher.InvokeShutdown();
                };

                callBack(win);
                win.Show();

                // make the thread support message pumping
                System.Windows.Threading.Dispatcher.Run();
            });

            // configure the thread
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();// this is like ShowDialog(), it blocks untill the user closes the window
        }

        /// <summary>
        /// Simpler, doesn't make the calling function wait
        /// </summary>
        /// <param name="callback"></param>
        public static void ShowWindowOutsideWPF(Action<System.Windows.Window> callback)
        {
            var t = new Thread(() =>
            {
                var win = new System.Windows.Window();

                callback(win);

                win.ShowDialog(); // this will cause this thread to wait, but not the calling function's thread
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start(); // returns back to caller, without waiting
        }
    }
}
