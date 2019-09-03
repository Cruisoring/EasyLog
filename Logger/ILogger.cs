using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Logger
{
    /// <summary>
    /// Main interface to describe capability expected from an <c>ILogger</c> instance.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Specify how message would be persistent.
        /// </summary>
        /// <param name="message">message to be saved as a log item.</param>
        void Save(String message);

        /// <summary>
        /// Get the minimum LogLevel to be logged by this ILogger.
        /// </summary>
        /// <returns>the min LogLevel that would enable the ILogger to save messages.</returns>
        LogLevel GetBottomLevel();

        /// <summary>
        /// Define if the message of the specific LogLevel shall be recorded.
        /// </summary>
        /// <param name="level">LogLevel of the concerned message.</param>
        /// <returns><tt>true</tt> means the message can be recorded, otherwise <tt>false</tt></returns>
        bool CanLog(LogLevel level);

        /// <summary>
        /// Get the message to be recorded as given LogLevel, with specific format and corresponding arguments.
        /// </summary>
        /// <param name="level">LogLevel of the message to be recorded as.</param>
        /// <param name="format">A format String.</param>
        /// <param name="args">Arguments referenced by the format specifiers in the format string.</param>
        /// <returns>A formatted string</returns>
        String GetMessage(LogLevel level, String format, params object[] args);
    }

    /// <summary>
    /// The container of extension methods of <c>ILogger</c> by converting default methods of Java io.github.cruisoring.logger.ILogger
    /// <see cref="https://github.com/Cruisoring/functionExtensions/blob/master/src/main/java/io/github/cruisoring/logger/ILogger.java"/>.
    /// </summary>
    public static class LoggerExtensions
    {
        public const int MinStackFrameCount = 2;
        public const string PercentageAscii = "&#37";

        //Specify how many stack frames to be captured, could be changed by external users at run-time
        public static Dictionary<LogLevel, int> StackFramesCount = new Dictionary<LogLevel, int>()
        {
            {LogLevel.VERBOSE, 11},
            {LogLevel.DEBUG, 9},
            {LogLevel.INFO, 7},
            {LogLevel.WARN, 5},
            {LogLevel.ERROR, 3},
            {LogLevel.NONE, 0},
        };

        static int GetFrameCount(LogLevel level) =>
            StackFramesCount.ContainsKey(level) ? StackFramesCount[level] : MinStackFrameCount;

        /// <summary>
        /// Try to call String.Format() and refrain potential IllegalFormatException by returning descriptive format+arguments message.
        /// </summary>
        /// <param name="format">template to compose a string with given arguments</param>
        /// <param name="args">arguments to be applied to the above template</param>
        /// <returns>string formatted with the given or exceptional template.</returns>
        public static string TryFormatString(String format, params object[] args)
        {
            //Special case when there is only one object[] argument provided, then it would be treated as intended arguments instead 
            if (args.Length == 1 && args[0] is object[])
            {
                args = (Object[])args[0];
            }
            try
            {
                String formatted = String.Format(format, args);
                formatted = formatted.Replace(PercentageAscii, "%");
                return formatted;
            }
            catch (Exception e)
            {
                List<string> orderedArgList = Enumerable.Range(0, args.Length)
                    .Select(i => $"[{i}]{(args[i] == null ? "null" : args[i].ToString())}")
                    .ToList();
                return string.Format("MalFormatted: format='{0}', args=[{1}]",
                    format == null ? "null" : format,
                    string.Join(",", orderedArgList)
                    );
            }
        }


        /// <summary>
        /// Log message composed by given format and arguments by checking if it is allowed by <c>CanLog(level)</c> first,
        /// if <tt>true</tt> then compose the message with given <c>format</c> and <c>args</c> to save as specific <c>level</c>. 
        /// </summary>
        /// 
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="level"> the <c>LogLevel</c> of the message to be recorded.</param>
        /// <param name="format"> the <c>format</c> part of String.Format() to be used to compose the final message</param>
        /// <param name="args">the optional <c>arguments</c> of String.Format() to be used to compose the final message</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Log(this ILogger logger, LogLevel level, String format, params object[] args)
        {
            if (logger.CanLog(level) && format != null)
            {
                string message = logger.GetMessage(level, format, args);
                logger.Save(message);
            }
            return logger;
        }

        /// <summary>
        /// Log Exception by checking if it is allowed by <c>CanLog(level)</c> first,
        /// if <tt>true</tt> then compose the message stackTrace of <c>ex</c> to save as specific <c>level</c>.
        /// </summary>
        /// 
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="level"> the <c>LogLevel</c> of the message to be recorded.</param>
        /// <param name="ex">The Exception to be logged.</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Log(this ILogger logger, LogLevel level, Exception ex)
        {
            if (logger.CanLog(level) && ex != null)
            {
                Exception cause = ex.GetBaseException() ?? ex;
                String stackTrace = StackTraceHelper.GetFilteredStackString(GetFrameCount(level), cause);
                String causeMessage = cause.Message;
                String message = cause != ex
                    ? $"{cause.GetType().Name} caused by {ex.GetType().Name}: {causeMessage}{Environment.NewLine}{stackTrace}"
                    : $"{cause.GetType().Name}: {causeMessage}{Environment.NewLine}{stackTrace}";
                Log(logger, level, message);
            }
            return logger;
        }

        /// <summary>
        /// To compose message with given <c>format</c> and <c>args</c> to save with <c>LogLevel.VERBOSE</c>
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="format"> the <c>format</c> part of String.Format() to be used to compose the final message</param>
        /// <param name="args">the optional <c>arguments</c> of String.Format() to be used to compose the final message</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Verbose(this ILogger logger, String format, params object[] args) =>
            Log(logger, LogLevel.VERBOSE, format, args);

        /// <summary>
        /// To compose message with given <c>format</c> and <c>args</c> to save with <c>LogLevel.DEBUG</c>
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="format"> the <c>format</c> part of String.Format() to be used to compose the final message</param>
        /// <param name="args">the optional <c>arguments</c> of String.Format() to be used to compose the final message</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Debug(this ILogger logger, String format, params object[] args) =>
            Log(logger, LogLevel.DEBUG, format, args);

        /// <summary>
        /// To compose message with given <c>format</c> and <c>args</c> to save with <c>LogLevel.INFO</c>
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="format"> the <c>format</c> part of String.Format() to be used to compose the final message</param>
        /// <param name="args">the optional <c>arguments</c> of String.Format() to be used to compose the final message</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Info(this ILogger logger, String format, params object[] args) =>
            Log(logger, LogLevel.INFO, format, args);

        /// <summary>
        /// To compose message with given <c>format</c> and <c>args</c> to save with <c>LogLevel.WARN</c>
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="format"> the <c>format</c> part of String.Format() to be used to compose the final message</param>
        /// <param name="args">the optional <c>arguments</c> of String.Format() to be used to compose the final message</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Warn(this ILogger logger, String format, params object[] args) =>
            Log(logger, LogLevel.WARN, format, args);

        /// <summary>
        /// To compose message with given <c>format</c> and <c>args</c> to save with <c>LogLevel.ERROR</c>
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="format"> the <c>format</c> part of String.Format() to be used to compose the final message</param>
        /// <param name="args">the optional <c>arguments</c> of String.Format() to be used to compose the final message</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Error(this ILogger logger, String format, params object[] args) =>
            Log(logger, LogLevel.ERROR, format, args);

        /// <summary>
        /// Log Exception as LogLevel.VERBOSE level.
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="ex">the <c>Exception</c> to be recorded.</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Verbose(this ILogger logger, Exception ex) => Log(logger, LogLevel.VERBOSE, ex);

        /// <summary>
        /// Log Exception as LogLevel.DEBUG level.
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="ex">the <c>Exception</c> to be recorded.</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Debug(this ILogger logger, Exception ex) => Log(logger, LogLevel.DEBUG, ex);

        /// <summary>
        /// Log Exception as LogLevel.INFO level.
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="ex">the <c>Exception</c> to be recorded.</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Info(this ILogger logger, Exception ex) => Log(logger, LogLevel.INFO, ex);

        /// <summary>
        /// Log Exception as LogLevel.WARN level.
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="ex">the <c>Exception</c> to be recorded.</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Warn(this ILogger logger, Exception ex) => Log(logger, LogLevel.WARN, ex);

        /// <summary>
        /// Log Exception as LogLevel.ERROR level.
        /// </summary>
        /// <param name="logger">The ILogger instance to be extended</param>
        /// <param name="ex">the <c>Exception</c> to be recorded.</param>
        /// <returns>ILogger instance to be used fluently.</returns>
        public static ILogger Error(this ILogger logger, Exception ex) => Log(logger, LogLevel.ERROR, ex);
    }
}
