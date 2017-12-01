using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WebPad.Dependencies.General.WPFUtilities.Run
{
    public static class Run
    {

        public static Task<RunResult> runWithUIThread(RunOnUIArgs args = null)
        {
            var promise = new TaskCompletionSource<RunResult>();

            var t = new Thread(() =>
            {
                try
                {
                    // followed some stuff here: http://reedcopsey.com/2011/11/28/launching-a-wpf-window-in-a-separate-thread-part-1/
                    SynchronizationContext.SetSynchronizationContext(
                                 new DispatcherSynchronizationContext(
                                     Dispatcher.CurrentDispatcher));

                    var win = new System.Windows.Window();

                    win.Closed += (_s, _args) =>
                    {
                        promise.SetResult(new RunResult
                        {
                            IsError = false
                        });
                        Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    };

                    win.Show();

                    // need #r "System.Windows.Presentation", and using System.WIndows.Threading to get extension to work
                    win.Dispatcher.BeginInvoke(() =>
                    {
                        // once the dispatcher is available then do this stuff
                        if (args != null && args.RunAfterWindowAvailable != null)
                        {
                            args.RunAfterWindowAvailable(win);
                        }
                    });

                    // Start the Dispatcher Processing
                    Dispatcher.Run();




                }
                catch (ThreadAbortException ex)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    promise.SetResult(new RunResult
                    {
                        IsError = true,
                        ex = ex
                    });
                }
            });

            t.TrySetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();

            return promise.Task;
        }




        
    }
}
