using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebPad.Dependencies.General.WPFUserControls.LogViewer
{
    public class Log4NetLogEntry : LogEntry
    {
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
