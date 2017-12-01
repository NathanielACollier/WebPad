using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Dependencies.General.Log4NetHelpers
{
    public static class CodeConfiguredUtilities
    {
        /// <summary>
        /// This function sets things up, and must be called first, then different appenders can be programatically added
        /// </summary>
        public static void InitializeLog4Net()
        {
            string configText = @"
						<log4net>
							<root>
							<level value='ALL' />   
							</root>
						</log4net>
						";

            log4net.LogManager.ResetConfiguration();
            // configure from stream
            System.IO.Stream configStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(configText));
            log4net.Config.XmlConfigurator.Configure(configStream);

        }

        public static bool IsLog4NetConfigured()
        {
            // source of this type of check is: http://neilkilbride.blogspot.com/2008/04/configure-log4net-only-once.html
            var repo = log4net.LogManager.GetRepository();

            return repo.Configured;
        }



        private const string DefaultLogPattern = "[%date{yyyy-MM-dd hh:mm:sstt}] %-5level %logger - %message%newline";


        public static void AddNotifyAppender(NotifyAppender.NewLogEntryHandler newLogEntryAction,
                                            log4net.Core.Level threshold = null,
                                            string logPattern = CodeConfiguredUtilities.DefaultLogPattern)
        {
            // add in our notify appender
            var repository = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();

            var logPatternLayout = new log4net.Layout.PatternLayout(logPattern);

            if (newLogEntryAction != null)
            {
                var notifyAppender = new NotifyAppender();
                notifyAppender.NewLogEntry += newLogEntryAction;

                if (threshold != null)
                {
                    notifyAppender.Threshold = threshold;
                }

                notifyAppender.Layout = logPatternLayout;
                notifyAppender.ActivateOptions();

                repository.Root.AddAppender(notifyAppender);
            }
        }

        /// <summary>
        /// See example at this place: http://aaubry.net/configuring-log4net-coloredconsoleappender-in-code.html
        /// </summary>
        /// <param name="threshold"></param>
        /// <param name="logPattern"></param>
        public static void AddColoredConsoleAppender(log4net.Core.Level threshold = null,
                                                        string logPattern = CodeConfiguredUtilities.DefaultLogPattern)
        {
            var repo = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();

            var coloredConsoleAppender = new log4net.Appender.ColoredConsoleAppender
            {
                Threshold = (threshold == null) ? log4net.Core.Level.All : threshold,
                Layout = new log4net.Layout.PatternLayout(logPattern)
            };

            coloredConsoleAppender.AddMapping(new log4net.Appender.ColoredConsoleAppender.LevelColors
            {
                Level = log4net.Core.Level.Info,
                ForeColor = log4net.Appender.ColoredConsoleAppender.Colors.White | log4net.Appender.ColoredConsoleAppender.Colors.HighIntensity
            });

            coloredConsoleAppender.AddMapping(new log4net.Appender.ColoredConsoleAppender.LevelColors
            {
                Level = log4net.Core.Level.Debug,
                ForeColor = log4net.Appender.ColoredConsoleAppender.Colors.White | log4net.Appender.ColoredConsoleAppender.Colors.HighIntensity,
                BackColor = log4net.Appender.ColoredConsoleAppender.Colors.Blue
            });

            coloredConsoleAppender.AddMapping(new log4net.Appender.ColoredConsoleAppender.LevelColors
            {
                Level = log4net.Core.Level.Warn,
                ForeColor = log4net.Appender.ColoredConsoleAppender.Colors.Yellow | log4net.Appender.ColoredConsoleAppender.Colors.HighIntensity,
                BackColor = log4net.Appender.ColoredConsoleAppender.Colors.Purple
            });

            coloredConsoleAppender.AddMapping(new log4net.Appender.ColoredConsoleAppender.LevelColors
            {
                Level = log4net.Core.Level.Error,
                ForeColor = log4net.Appender.ColoredConsoleAppender.Colors.Yellow | log4net.Appender.ColoredConsoleAppender.Colors.HighIntensity,
                BackColor = log4net.Appender.ColoredConsoleAppender.Colors.Red
            });


            coloredConsoleAppender.ActivateOptions();
            repo.Root.AddAppender(coloredConsoleAppender);
        }


        public static void AddRollingFileAppender(string filePath = "logs/log.txt",
                                           log4net.Core.Level threshold = null,
                                           string logPattern = CodeConfiguredUtilities.DefaultLogPattern,
                                            log4net.Appender.RollingFileAppender.RollingMode rollingMode = log4net.Appender.RollingFileAppender.RollingMode.Date,
                                            string rollingModeDatePattern = "yyyyMMdd",
                                            int maxNumberOfLogFilesToKeep = 5)
        {
            // it's cast to a Hierarchy because we need repo.Root to add appenders
            var repo = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();

            var logPatternLayout = new log4net.Layout.PatternLayout(logPattern);

            var fileAppender = new log4net.Appender.RollingFileAppender();

            fileAppender.File = filePath;
            fileAppender.AppendToFile = true;
            fileAppender.Layout = logPatternLayout;
            fileAppender.MaxSizeRollBackups = maxNumberOfLogFilesToKeep; // http://stackoverflow.com/questions/95286/log4net-set-max-backup-files-on-rollingfileappender-with-rolling-date

            fileAppender.RollingStyle = rollingMode;

            if (rollingMode == log4net.Appender.RollingFileAppender.RollingMode.Date && !string.IsNullOrEmpty(rollingModeDatePattern))
            {
                fileAppender.DatePattern = "yyyyMMdd";
            }

            if (threshold != null)
            {
                fileAppender.Threshold = threshold;
            }

            fileAppender.ActivateOptions();
            repo.Root.AddAppender(fileAppender);
        }



        public static void AddSmtpAppender(string toAddress, string fromAddress, string subject, string smtpHost,
                                            int smtpPort = 25,
                                                int bufferSize = 100000,
                                               log4net.Core.Level threshold = null,
                                           string logPattern = CodeConfiguredUtilities.DefaultLogPattern)
        {
            // it's cast to a Hierarchy because we need repo.Root to add appenders
            var repo = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();

            var logPatternLayout = new log4net.Layout.PatternLayout(logPattern);

            var smtpAppender = new log4net.Appender.SmtpAppender();

            smtpAppender.Layout = logPatternLayout;

            smtpAppender.To = toAddress;
            smtpAppender.From = fromAddress;
            smtpAppender.Subject = subject;
            smtpAppender.SmtpHost = smtpHost;
            smtpAppender.Port = smtpPort;
            smtpAppender.BufferSize = bufferSize;

            if (threshold != null)
            {
                smtpAppender.Threshold = threshold;
            }

            smtpAppender.ActivateOptions();
            repo.Root.AddAppender(smtpAppender);
        }



        private static System.Net.IPAddress GetInterNetworkIPv4AddressFromHostName(string hostname)
        {
            return System.Net.Dns.GetHostEntry(hostname)
                        .AddressList
                        .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }

        public static void AddUdpAppenderWithLog4JFormat(int remotePort)
        {
            var localIPAddress = GetInterNetworkIPv4AddressFromHostName(System.Net.Dns.GetHostName());

            AddUdpAppenderWithLog4JFormat(localIPAddress, remotePort);
        }


        /// <summary>
        /// This does a lookup on the address you enter.  Right now exceptions are not caught...
        /// </summary>
        /// <param name="remoteAddress"></param>
        /// <param name="remotePort"></param>
        public static void AddUdpAppenderWithLog4JFormat(string remoteAddress, int remotePort)
        {
            var ipAddress = GetInterNetworkIPv4AddressFromHostName(remoteAddress);
            // GetHostAddresses is going to throw an exception if you enter a bad address.  We need to decide if we want to forward that exception or catch it...
            AddUdpAppenderWithLog4JFormat(ipAddress, remotePort);
        }

        public static void AddUdpAppenderWithLog4JFormat(System.Net.IPAddress remoteAddress, int remotePort)
        {
            var repo = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();

            var layout = new log4net.Layout.XmlLayoutSchemaLog4j(true); // locationInformation is the true/false parameter

            // log pattern and all that type stuff are for files, with UDP we are sending xml info
            var udpAppender = new log4net.Appender.UdpAppender();
            udpAppender.Layout = layout;
            udpAppender.RemoteAddress = remoteAddress;
            udpAppender.RemotePort = remotePort;

            udpAppender.ActivateOptions();
            repo.Root.AddAppender(udpAppender);
        }






    }
}
