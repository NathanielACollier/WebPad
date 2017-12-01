using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WebPad.Dependencies.General.WPFUserControls.LogViewer
{
    public class CollapsibleLogEntry : LogEntry
    {
        public ObservableCollection<LogEntry> Contents
        {
            get { return GetValue(() => this.Contents); }
        }


    }


}
