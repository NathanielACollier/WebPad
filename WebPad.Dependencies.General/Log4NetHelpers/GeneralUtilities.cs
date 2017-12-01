using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Dependencies.General.Log4NetHelpers
{
    public static class GeneralUtilities
    {

        public static void FlushAppenderBuffers()
        {
            var repository = log4net.LogManager.GetRepository();
            foreach (var appender in repository.GetAppenders().Where(app => app is log4net.Appender.BufferingAppenderSkeleton))
            {
                var buffer = (log4net.Appender.BufferingAppenderSkeleton)appender;
                if (buffer != null)
                {
                    buffer.Flush();
                }
            }
        }


        public static void InitializeLog4NetFromConfigFile(string configFilePath)
        {
            log4net.LogManager.ResetConfiguration();

            var fileInfo = new System.IO.FileInfo(configFilePath);

            log4net.Config.XmlConfigurator.ConfigureAndWatch(fileInfo);
        }

    }
}
