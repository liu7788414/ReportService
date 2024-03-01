using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportService
{
    public static class logHelper
    {
        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("InfoLog");
        public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("ErrorLog");
        public static void LogInfo(string info)
        {
            if (loginfo.IsInfoEnabled)
            {
                Console.WriteLine(info);
                loginfo.Info(info);
            }
        }
        public static void LogError(string info, Exception ex)
        {
            if (logerror.IsErrorEnabled)
            {
                logerror.Error(info, ex);
            }
        }

    }
}
