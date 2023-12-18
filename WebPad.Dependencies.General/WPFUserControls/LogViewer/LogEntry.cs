using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Dependencies.General.WPFUserControls.LogViewer
{
    internal class LogEntry : nac.ViewModelBase.ViewModelBase
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


        public string Level
        {
            get { return GetValue(() => this.Level); }
            set { SetValue(() => this.Level, value); }
        }

        public string LoggerName
        {
            get { return GetValue(() => this.LoggerName); }
            set { SetValue(() => this.LoggerName, value); }
        }

    }
}
