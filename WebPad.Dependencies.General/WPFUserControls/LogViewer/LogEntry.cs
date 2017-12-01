using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Dependencies.General.WPFUserControls.LogViewer
{
    public class LogEntry : WPFViewModelBase.ViewModelBase
    {
        public DateTime DateTime
        {
            get { return GetValue(() => this.DateTime); }
            set { SetValue(() => this.DateTime, value); }
        }

        public int Index
        {
            get { return GetValue(() => this.Index); }
            set { SetValue(() => this.Index, value); }
        }

        public string Message
        {
            get { return GetValue(() => this.Message); }
            set { SetValue(() => this.Message, value); }
        }



        public bool IsMessageCopyable
        {
            get { return GetValue(() => this.IsMessageCopyable); }
            set { SetValue(() => this.IsMessageCopyable, value); }
        }


    }
}
