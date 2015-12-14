using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLogger
{
    //Delegate of general message logging handler
    public delegate void PerformLogging(LogLevel logLevel, string message);

    //Delegate of Exception logging handler
    public delegate void PeformExceptionLogging(Exception exception, StackTrace stacktrace, LogLevel? stacktraceLevel = null);

    public class Log : ILog, IDisposable
    {
        #region Constants Defintions
        public const string SpaceBeforeTag = "  ";
        public const string TagMessageConnector = ": ";
        public const string Unknown = "Unknown";
        #endregion

        #region Events and Default Handlers
        public static PerformLogging DefaultDoLogging = (level, msg) => Console.WriteLine(msg);

        private static event PerformLogging onLoggingEvent;
        public static event PerformLogging OnLoggingEvent
        {
            add
            {
                lock (DefaultDoLogging)
                {
                    if (onLoggingEvent == null || !onLoggingEvent.GetInvocationList().Contains(value))
                    {
                        onLoggingEvent += value;
                    }
                }
            }
            remove
            {
                lock (DefaultDoLogging)
                {
                    if (value != null && onLoggingEvent != null && onLoggingEvent.GetInvocationList().Contains(value))
                    {
                        onLoggingEvent -= value;
                    }
                }
            }
        }

        private static event PeformExceptionLogging onExceptionEvent;
        public static event PeformExceptionLogging OnExceptionEvent
        {
            add
            {
                lock (DefaultDoLogging)
                {
                    if (onExceptionEvent == null || !onExceptionEvent.GetInvocationList().Contains(value))
                    {
                        onExceptionEvent += value;
                    }
                }
            }
            remove
            {
                lock (DefaultDoLogging)
                {
                    if (value != null && onExceptionEvent != null && onExceptionEvent.GetInvocationList().Contains(value))
                    {
                        onExceptionEvent -= value;
                    }
                }
            }
        }

        #endregion

        #region Default Logging Settings
        public static LogLevel DefaultLogLevel = LogLevel.DEBUG_AND_ABOVE;
        public static Func<LogLevel, string> DefaultTag = FormattedTag;
        public static Func<string, string, string> DefaultFinalMessage =
            (tag, details) => string.Format("{0}{1}{2}", tag, TagMessageConnector, details);
        public static Func<Exception, StackTrace, string> DefaultStacktrace = DebuggableStacktrace;

        public static string ByException(Exception exception, StackTrace stacktrace)
        {
            return exception.StackTrace;
        }

        public static string ByStacktrace(Exception exception, StackTrace stacktrace)
        {
            return stacktrace.ToString();
        }

        public static string DebuggableStacktrace(Exception exception, StackTrace stacktrace)
        {
            if (stacktrace == null)
                throw new ArgumentNullException();
            StringBuilder sb = new StringBuilder();
            StackFrame[] frames = stacktrace.GetFrames();
            int i = 0;
            foreach(StackFrame frame in frames)
            {
                string fileName = frame.GetFileName();
                if (fileName == logClassFilename)
                    continue;

                int lineNumber = frame.GetFileLineNumber();
                if(lineNumber > 0)
                {
                    sb.AppendFormat("{0}{1}: {2}, line {3}\r\n", new string(' ', (++i)*2), frame.GetMethod().Name, fileName, lineNumber);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Default tag factory based on the LogLevel.
        /// </summary>
        /// <param name="logLevel">Level of the message to be logged.</param>
        /// <returns>Formatted string of the Tag to differentiate various LogLevel.</returns>
        public static string FormattedTag(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.VERBOSE:
                case LogLevel.DEBUG:
                    return string.Format("{0}[{1}]", SpaceBeforeTag, logLevel.ToString().First());
                case LogLevel.INFO:
                    return string.Format("[I:{0}]", ElapsedTimeString());
                case LogLevel.WARN:
                    return string.Format("*[W:{0}]", ElapsedTimeString());
                case LogLevel.ERROR:
                    return string.Format("***[ERROR:{0}@{1}]", ElapsedTimeString(), DateTime.Now.ToString("HH:mm:ss"));
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Predefined tag factory to neglect LogLevel.
        /// </summary>
        /// <param name="logLevel">Level of the message to be logged.</param>
        /// <returns>Empty string to hide LogLevel.</returns>
        public static string NoTag(LogLevel logLevel)
        {
            return string.Empty;
        }

        /// <summary>
        /// Predefined short-formed tag factory.
        /// </summary>
        /// <param name="logLevel">Level of the message to be logged.</param>
        /// <returns>Tag of the LogLevel.</returns>
        public static string ShortTag(LogLevel logLevel)
        {
            return string.Format("[{0}]", logLevel.ToString().First());
        }

        /// <summary>
        /// Predefined full tag factory.
        /// </summary>
        /// <param name="logLevel">Level of the message to be logged.</param>
        /// <returns>Tag of the LogLevel.</returns>
        public static string FullTag(LogLevel logLevel)
        {
            return string.Format("[{0}]", logLevel);
        }

        #endregion

        #region Stopwatch instance and helper methods
        private static Stopwatch stopwatch = new Stopwatch();

        protected static long createMoment = DateTime.Now.Ticks;
        protected static Dictionary<string, List<long>> MomentsRepository = new Dictionary<string, List<long>>();

        public static ICollection<string> GetMomentIds()
        {
            return MomentsRepository.Keys;
        }

        public static int MarkMoment(string momentId = null)
        {
            if (momentId == null)
            {
                StackTrace stacktrace = new StackTrace(true);
                StackFrame callerFrame = stacktrace.GetFrames().FirstOrDefault(frame =>
                    frame.GetFileName() != logClassFilename && frame.GetFileLineNumber() != 0);
                momentId = callerFrame == null ? "Unknown"
                    : string.Format("{0}: {1}_L{2}", callerFrame.GetFileName(), callerFrame.GetMethod().Name, callerFrame.GetFileLineNumber());
            }
            if (!MomentsRepository.ContainsKey(momentId))
            {
                MomentsRepository.Add(momentId, new List<long>());
            }
            long ticks = DateTime.Now.Ticks;
            List<long> moments = MomentsRepository[momentId];
            moments.Add(ticks);
            return moments.Count;
        }

        public static long[] GetMoments(string momentId, Func<long, int, bool>predicate = null)
        {
            if (momentId==null)
                throw new ArgumentNullException("momentId cannot be null to retrieve the concerned moments.");
            //Returns empty array if no such moments recorded
            if (!MomentsRepository.ContainsKey(momentId))
                return new long[0];
            
            if (predicate == null)
                return MomentsRepository[momentId].ToArray();
            
            var selected = MomentsRepository[momentId].Where((moment, index) => predicate(moment, index)).ToArray();
            return selected;
        }

        public static long[] GetMoments(string momentId, IEnumerable<int> indexes=null)
        {
            if (momentId == null)
                throw new ArgumentNullException("momentId cannot be null to retrieve the concerned moments.");
            //Returns empty array if no such moments recorded
            if (!MomentsRepository.ContainsKey(momentId))
                return new long[0];

            if (indexes == null)
                return MomentsRepository[momentId].ToArray();

            var moments = MomentsRepository[momentId];
            List<int> qualifiedIndexes = indexes.Where(i => i >= 0 && i < moments.Count).ToList();
            var result = qualifiedIndexes.Select(i => moments[i]).ToArray();
            return result;
        }
        
        public static long[] GetIntervals(string momentId, IEnumerable<int> indexes=null)
        {
            if (momentId == null)
                throw new ArgumentNullException("momentId cannot be null to retrieve the concerned moments.");
            //Returns empty array if no such moments recorded
            if (!MomentsRepository.ContainsKey(momentId))
                return new long[0];

            var moments = MomentsRepository[momentId];
            if (indexes == null)
            {
                var intervals = moments.Skip(1).Select((v, index) => v - moments[index-1]).ToArray();
                return intervals;
            }
            List<int> qualifiedIndexes = indexes.Where(i => i > 0 && i < moments.Count).ToList();

            long[] selectedIntervals = qualifiedIndexes.Select(i => moments[i] - moments[i-1]).ToArray();
            return selectedIntervals;
        }

        public static void RestartStopwatch()
        {
            raiseEvent(LogLevel.INFO, string.Format("Restart stopwatch at {0}, with time elapsed of {1}", DateTime.Now, ElapsedTimeString()));
            stopwatch.Restart();
        }

        public static string ElapsedTimeString(string format = null)
        {
            TimeSpan elapsed = stopwatch.Elapsed;
            return AsString(elapsed, format);
        }

        private static string AsString(TimeSpan elapsed, string format=null)
        {
            if (format != null)
                return elapsed.ToString(format);

            if (elapsed < TimeSpan.FromSeconds(1))
                return elapsed.ToString(@"fff") + "ms";
            else if (elapsed < TimeSpan.FromSeconds(10))
                return elapsed.ToString(@"ss\.fff") + "s";
            else if (elapsed < TimeSpan.FromMinutes(10))
                return elapsed.ToString(@"mm\:ss\.fff");
            else if (elapsed < TimeSpan.FromHours(24))
                return elapsed.ToString(@"h'h 'm'm 's's'");
            else if (elapsed < TimeSpan.FromDays(24))
                return elapsed.ToString(@"h'h 'm'm 's's'");
            else if (elapsed < TimeSpan.FromDays(99))
                return elapsed.ToString(@"dd\\.hh\\:mm\\:ss");
            else
                return elapsed.ToString("g");
        }
        #endregion

        // Default Logger to be used even without explict calling of new Log()
        public readonly static Log Instance = new Log();

        protected static string logClassFilename = null;
        static Log()
        {
            RestartStopwatch();

            StackTrace stackTrace = new StackTrace(true);
            StackFrame thisFrame = stackTrace.GetFrame(0);
            logClassFilename = thisFrame.GetFileName();
        }

        public static LogLevel ChangeDefaultLogLevel(LogLevel newLogLevel)
        {
            LogLevel currentDefaultLogLevel = DefaultLogLevel;
            DefaultLogLevel = newLogLevel;
            //Change the MinLogLevel of the default Log instance
            Instance.ConcernedLogs = newLogLevel;
            //Return the previous setting as result
            return currentDefaultLogLevel;
        }

        /// <summary>
        /// Raise Logging Event to allow all EventHandler to record the details.
        /// </summary>
        /// <param name="level">LogLevel of the message.</param>
        /// <param name="details">Message to be logged.</param>
        private static void raiseEvent(LogLevel level, string details)
        {
            try
            {
                if (onLoggingEvent != null)
                    onLoggingEvent(level, details);
            }
            catch (Exception ex)
            {
                //Assume the default log is still working
                try
                {
                    Instance.e(ex, LogLevel.INFO);
                }
                catch { }
            }
        }

        /// <summary>
        /// Raise Logging Event to allow all EventHandler to record the message with format and args.
        /// </summary>
        /// <param name="level">LogLevel of the message.</param>
        /// <param name="format">Format to compose the message.</param>
        /// <param name="args">Arguments to be filled in the format string.</param>
        private static void raiseEvent(LogLevel level, string format, params object[] args)
        {
            try
            {
                if (format == null)
                    throw new ArgumentNullException("format cannot be null!");
                if (onLoggingEvent != null)
                {
                    string details = string.Format(format, args);
                    onLoggingEvent(level, details);
                }
            }
            catch (Exception ex)
            {
                //Assume the default log is still working
                try
                {
                    Instance.e(ex, LogLevel.INFO);
                }
                catch { }
            }
        }

        private static void raiseEvent(Exception exception, StackTrace stacktrace, LogLevel? stacktraceLevel = null)
        {
            try
            {
                if (onExceptionEvent != null)
                {
                    onExceptionEvent(exception, stacktrace, stacktraceLevel);
                }
            }
            catch (Exception ex)
            {
                //Assume the default log is still working
                try
                {
                    Instance.e(ex, LogLevel.INFO);
                }
                catch { }
            }
        }

        #region Static Methods encapsulating LoggingEvent triggering
        public static void V(string details)
        {
            raiseEvent(LogLevel.VERBOSE, details);
        }

        public static void V(string format, params object[] args)
        {
            raiseEvent(LogLevel.VERBOSE, format, args);
        }

        public static void D(string details)
        {
            raiseEvent(LogLevel.DEBUG, details);
        }

        public static void D(string format, params object[] args)
        {
            raiseEvent(LogLevel.DEBUG, format, args);
        }

        public static void I(string details)
        {
            raiseEvent(LogLevel.INFO, details);
        }

        public static void I(string format, params object[] args)
        {
            raiseEvent(LogLevel.INFO, format, args);
        }

        public static void W(string details)
        {
            raiseEvent(LogLevel.WARN, details);
        }

        public static void W(string format, params object[] args)
        {
            raiseEvent(LogLevel.WARN, format, args);
        }

        public static void E(string details)
        {
            raiseEvent(LogLevel.ERROR, details);
        }

        public static void E(Exception ex, LogLevel? stackTraceLevel = LogLevel.INFO)
        {
            raiseEvent(ex, new StackTrace(true), stackTraceLevel);
        }
        #endregion

        #region Properties and Hooks
        public LogLevel ConcernedLogs { get; set; }
        public PerformLogging DoLogging { get; protected set; }
        public Func<LogLevel, string> BuildTag { get; protected set; }
        public Func<string, string, string> BuildFinalMessage { get; protected set; }
        public Func<Exception, StackTrace, string> BuildStacktrace { get; protected set; }
        public string Descripiton { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of default Logging behaviours.
        /// </summary>
        protected Log()
            : this(DefaultLogLevel, DefaultDoLogging)
        {
        }

        public Log(LogLevel concernedLogs, PerformLogging doLogging, 
            Func<LogLevel, string> buildTag=null
            , Func<Exception, StackTrace, string> buildStacktrace = null
            , Func<string, string, string> buildMessage = null
            , string description = null)
        {
            if (doLogging == null)
                throw new ArgumentNullException("Logging means must be specified!");
            this.ConcernedLogs = concernedLogs;
            this.DoLogging = doLogging;
            this.BuildTag = buildTag ?? DefaultTag;
            this.BuildFinalMessage = buildMessage??DefaultFinalMessage;
            this.BuildStacktrace = buildStacktrace??DefaultStacktrace;
            this.Descripiton = description ?? string.Format("Log{0}", this.GetHashCode());

            OnLoggingEvent += this.log;
            OnExceptionEvent += this.logException;
        }

        #endregion

        #region Instance Methods
        /// <summary>
        /// Centralized mechanism to create tag based on the LogLevel and compose
        /// and log the final message.
        /// </summary>
        /// <param name="logLevel">Level of the logging request.</param>
        /// <param name="details">Logging details to be reserved.</param>
        protected virtual void log(LogLevel logLevel, string details)
        {
            if (DoLogging != null && (logLevel & ConcernedLogs) != 0)
            {
                string tag = BuildTag(logLevel);
                switch(logLevel)
                {
                    case LogLevel.VERBOSE:
                        v(tag, details);
                        break;
                    case LogLevel.DEBUG:
                        d(tag, details);
                        break;
                    case LogLevel.INFO:
                        i(tag, details);
                        break;
                    case LogLevel.WARN:
                        w(tag, details);
                        break;
                    case LogLevel.ERROR:
                        e(tag, details);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        protected virtual void logException(Exception exception, StackTrace stacktrace, LogLevel? stacktraceLevel = null)
        {
            if(exception == null || DoLogging == null)
                return;

            log(LogLevel.ERROR, exception.Message);

            if (!stacktraceLevel.HasValue || BuildStacktrace == null)
                return;
            LogLevel level = stacktraceLevel.Value;
            if ((level & ConcernedLogs) == 0)
                return;
            string stacktraceString = BuildStacktrace(exception, stacktrace);
            log(level, "StackTrace\r\n" + stacktraceString);
        }

        public virtual void v(string tag, string details)
        {
            DoLogging(LogLevel.VERBOSE, BuildFinalMessage(tag, details));
        }

        public virtual void d(string tag, string details)
        {
            DoLogging(LogLevel.DEBUG, BuildFinalMessage(tag, details));
        }

        public virtual void i(string tag, string details)
        {
            DoLogging(LogLevel.INFO, BuildFinalMessage(tag, details));
        }

        public virtual void w(string tag, string details)
        {
            DoLogging(LogLevel.WARN, BuildFinalMessage(tag, details));
        }

        public virtual void e(string tag, string details)
        {
            DoLogging(LogLevel.ERROR, BuildFinalMessage(tag, details));
        }

        public virtual void v(string details)
        {
            v(BuildTag(LogLevel.VERBOSE), details);
        }

        public virtual void d(string details)
        {
            d(BuildTag(LogLevel.DEBUG), details);
        }

        public virtual void i(string details)
        {
            i(BuildTag(LogLevel.INFO), details);
        }

        public virtual void w(string details)
        {
            w(BuildTag(LogLevel.WARN), details);
        }

        public virtual void e(string details)
        {
            e(BuildTag(LogLevel.ERROR), details);
        }

        public virtual void e(Exception exception, LogLevel? stacktraceLevel = null)
        {
            logException(exception, new StackTrace(true), stacktraceLevel);
        }

        public override string ToString()
        {
            return ConcernedLogs.ToString();
        }

        private bool disposed = false;

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                    //Simply remove the event handler
                    OnLoggingEvent -= this.log;
                    OnExceptionEvent -= this.logException;
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~Log()
        {
            // Simply call Dispose(false).
            Dispose (false);
        }

        #endregion Instance Methods
    }
}

