using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using log4net.Appender;
using log4net.Core;

namespace WebPad.Dependencies.General.Log4NetHelpers
{
    public class NotifyAppender : AppenderSkeleton
    {

        public delegate void NewLogEntryHandler(object sender, NewLogEntryEventArgs e);
        public event NewLogEntryHandler NewLogEntry;

        /// <summary>
        /// Append the log information to the notification.
        /// </summary>
        /// <param name="loggingEvent">The log event.</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            Layout.Format(writer, loggingEvent);

            if (NewLogEntry != null)
            {
                NewLogEntry(this, new NewLogEntryEventArgs
                {
                    FormattedMessage = writer.ToString(),
                    SourceEvent = loggingEvent
                });
            }
        }

    }
}
