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

using System.Windows.Threading; // this is for extension methods

using System.Windows.Media.Animation; // needed for story board stuff

namespace WebPad.Dependencies.General.WPFUserControls.WpfBusyIndicator
{

    /// <summary>
    /// Interaction logic for BusyIndicator.xaml
    /// </summary>
    public partial class BusyIndicatorControl : UserControl
    {


        #region Busy DP
        public static readonly DependencyProperty BusyProperty = DependencyProperty.Register("Busy",
                        typeof(bool),
                        typeof(BusyIndicatorControl),
                        new UIPropertyMetadata
                        {
                            PropertyChangedCallback = BusyPropertyChanged,
                            CoerceValueCallback = CoerceBusyProperty
                        });

        /// <summary>
        /// Using the dependency property method is very unreliable.  It can only do busy or not busy.
        /// 
        /// Mixing the function calls is not supported.
        /// 
        /// In most GUI apps use the function calls.  Where binding is the only way the dependency property is available.
        /// </summary>
        public bool Busy
        {
            get { return (bool)GetValue(BusyProperty); }
            set { SetValue(BusyProperty, value); }
        }

        private static void BusyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BusyIndicatorControl busyControl = (BusyIndicatorControl)d;

        }


        //See: http://blog.ningzhang.org/2008/11/dependencyproperty-validation-coercion.html
        /// <summary>
        /// This ensures the Busy will not be false, untill the last Busy call is dealt with
        /// It will keep the value true untill the counter is 0
        /// </summary>
        /// <param name="d"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object CoerceBusyProperty(DependencyObject d, object value)
        {
            BusyIndicatorControl busyControl = (BusyIndicatorControl)d;

            bool val = (bool)value;

            if (val == false && busyControl.BusyMode == BusyMode.BusyIfTrue)
            {
                // if the user set it to false, then HideBusy will decriment the counter, and determine if this is the last one
                //  if it is the last one we will get back a false, if it isn't we will leave the busy value at true
                return busyControl.HideBusy();
            }
            else if (val == true && busyControl.BusyMode == BusyMode.BusyIfTrue)
            {
                busyControl.ShowBusy();
                return true;
            }
            else if (val == false && busyControl.BusyMode == BusyMode.BusyIfFalse)
            {
                busyControl.ShowBusy();
                return true;
            }
            else if (val == true && busyControl.BusyMode == BusyMode.BusyIfFalse)
            {
                return busyControl.HideBusy();
            }

            throw new NotImplementedException("Should never reach this point.  All conditions should be covered");
        }



        #endregion


        /// <summary>
        /// Default to busy if true. Only used in the bind method
        /// 
        /// It's ignored if you use the function calls.
        /// </summary>
        public BusyMode BusyMode { get; set; }




        #region Constructor
        public BusyIndicatorControl()
        {
            this.BusyMode = WpfBusyIndicator.BusyMode.BusyIfTrue; // default to busy if true
            InitializeComponent();
        }
        #endregion

        #region Animation Control Functions

        private void Start()
        {
            Storyboard SpinningAnimation = (Storyboard)FindResource("SpinningAnimation");
            SpinningAnimation.Begin(this);
        }

        private void Stop()
        {
            Storyboard SpinningAnimation = (Storyboard)FindResource("SpinningAnimation");
            SpinningAnimation.Stop();
        }

        private void Pause()
        {
            Storyboard SpinningAnimation = (Storyboard)FindResource("SpinningAnimation");
            SpinningAnimation.Pause();
        }

        private void Resume()
        {
            Storyboard SpinningAnimation = (Storyboard)FindResource("SpinningAnimation");
            SpinningAnimation.Resume();
        }

        #endregion




        private int numberOfBusyCounter = 0;


        /// <summary>
        /// This will be used to start the busy indicator
        /// </summary>
        /// <param name="numberOfBusyCounter"></param>
        /// <param name="showAction"></param>
        public void ShowBusy(Action actionToRunWhenBusyStarted = null)
        {
            if (numberOfBusyCounter == 0)
            {
                this.Visibility = System.Windows.Visibility.Visible;
                this.Start();

                if (actionToRunWhenBusyStarted != null)
                {
                    actionToRunWhenBusyStarted.Invoke();
                }
            }

            ++numberOfBusyCounter;
        }



        /// <summary>
        /// This will be called when you are ready to turn off the busy animation
        /// </summary>
        /// <param name="actionToRunWhenBusyFinished"></param>
        /// <returns>A value indicating if we are still busy or not.  It is possible HideBusy could be called multiple times and still be busy.</returns>
        public bool HideBusy(Action actionToRunWhenBusyFinished = null)
        {
            bool result = false;

            if (--numberOfBusyCounter <= 0)
            {
                this.Stop();

                this.Dispatcher.Invoke(() =>
                {
                    this.Visibility = System.Windows.Visibility.Collapsed;
                });


                if (actionToRunWhenBusyFinished != null)
                {
                    actionToRunWhenBusyFinished.Invoke();
                }

                numberOfBusyCounter = 0;

                result = false;
            }

            return result;
        }



    }
}
