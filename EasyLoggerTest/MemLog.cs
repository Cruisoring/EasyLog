using EasyLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoggerTest
{
    public class MemLog : Log
    {
        StringBuilder sb = new StringBuilder();

        public MemLog(LogLevel minLogLevel,
            Func<LogLevel, string> buildTag = null
            , Func<Exception, StackTrace, string> buildStacktrace = null
            , Func<string, string, string> buildMessage = null)
        {
            this.ConcernedLogs = minLogLevel;
            this.DoLogging = (level, msg) => sb.Insert(0, msg + "\r\n");
            this.BuildTag = buildTag ?? DefaultTag;
            this.BuildFinalMessage = buildMessage ?? DefaultFinalMessage;
            this.BuildStacktrace = buildStacktrace;

            OnLoggingEvent += this.log;
            OnExceptionEvent += this.logException;
        }

        public string GetLog()
        {
            return sb.ToString();
        }
    }
}
