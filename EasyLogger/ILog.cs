using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLogger
{
    public interface ILog
    {
        //Hook method to define the means to record the message
        PerformLogging DoLogging { get; }

        //Min LogLevel to be recorded by calling DoLogging
        LogLevel ConcernedLogs { get; set; }

        //Hook method to uniformly generate tags of the specific LogLevels.
        Func<LogLevel, string> BuildTag { get; }

        //Hook method to convert the stacktrace of an Exception 
        Func<Exception, StackTrace, string> BuildStacktrace { get; }

        //Hook method to combine TAG and DETAILS together as messages to be recorded
        Func<string, string, string> BuildFinalMessage { get; }


        //Method to log VERBOSE level details with default TAG
        void v(string details);
        //Method to log DEBUG level details with default TAG
        void d(string details);
        //Method to log INFO level details with default TAG
        void i(string details);
        //Method to log WARN level details with default TAG
        void w(string details);
        //Method to log ERROR level details with default TAG
        void e(string details);


        //Method to log VERBOSE level details with customer specific TAG
        void v(string tag, string details);
        //Method to log DEBUG level details with customer specific TAG
        void d(string tag, string details);
        //Method to log INFO level details with customer specific TAG
        void i(string tag, string details);
        //Method to log WARN level details with customer specific TAG
        void w(string tag, string details);
        //Method to log ERROR level details with customer specific TAG
        void e(string tag, string details);

        //Method to log exception
        void e(Exception exception, LogLevel? stacktraceLevel = null);
    }
}
