using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Logger
{
    /// <summary>
    /// Class to extract meaning StackTrace information by filtering noises of the System, low-level frameworks (NUnit, MSTest and etc.) 
    /// </summary>
    public static class StackTraceHelper
    {
        //Default number of StackFrames to be captured
        public const int DefaultFrameCountToCapture = 5;

        /// <summary>
        /// This instance would be used by Regex to match the method fullname to get meaning StackFrames helpful for trouble-shooting.
        /// Feel free to update it with the concerned packages that shall be neglected.
        /// </summary>
        public static HashSet<string> DefaultStackFilters = new HashSet<string>()
        {
            @"^System\.",
            @"^Microsoft\.",
            @"NUnit\.",
            $"{nameof(StackTraceHelper)}"
        };

        /// <summary>
        /// Get StackTrace of either current process execution or the Exception if given to extract all meaningful StackFrames with their orders.
        /// </summary>
        /// <param name="maxCount"><c>Optional</c> Max number of meaningful StackFrames to be extracted, the <c>DefaultFrameCountToCapture</c> would be used if not applied.</param>
        /// <param name="ex"><c>Optional</c> Exception used to get the StackTrace, capture current execution if not provided.</param>
        /// <param name="filters"><c>Optional</c> Regex filters to screen out meaningless StackFrames, <c>DefaultStackFilters</c> would be used by default.</param>
        /// <returns>A list of StackFrame with their original order information as Tuples.</returns>
        public static List<Tuple<int, StackFrame>> GetFilteredStacks(int maxCount = DefaultFrameCountToCapture, Exception ex = null,
            ICollection<string> filters = null)
        {
            if (maxCount == 0)
            {
                return new List<Tuple<int, StackFrame>>();
            }

            filters = filters ?? DefaultStackFilters;

            StackTrace stackTrace = ex == null ? new StackTrace(true) : new StackTrace(ex);
            StackFrame[] stackFrames = stackTrace.GetFrames();
            if (stackFrames == null || stackFrames.Length == 0)
            {
                return new List<Tuple<int, StackFrame>>();
            }

            List<Tuple<int, StackFrame>> orderedFrames = stackFrames
                .Where(sf => sf != null && !filters.Any(
                    filter => Regex.IsMatch(sf.GetMethod().ReflectedType.FullName, filter)))
                .Take(maxCount)
                .Select(f => Tuple.Create(Array.IndexOf(stackFrames, f), f))
                .ToList();

            return orderedFrames;
        }

        /// <summary>
        /// Get descriptive info of the meaningful StackFrames of either current process execution
        /// or the Exception if given to extract all meaningful StackFrames with their orders.
        /// </summary>
        /// <param name="maxCount"><c>Optional</c> Max number of meaningful StackFrames to be extracted, the <c>DefaultFrameCountToCapture</c> would be used if not applied.</param>
        /// <param name="ex"><c>Optional</c> Exception used to get the StackTrace, capture current execution if not provided.</param>
        /// <param name="filters"><c>Optional</c> Regex filters to screen out meaningless StackFrames, <c>DefaultStackFilters</c> would be used by default.</param>
        /// <param name="indent"><c>Optional</c> whitespace char to indent the lines of stackframe, <c>SPACE</c> is used as the default.</param>
        /// <returns></returns>
        public static string GetFilteredStackString(int maxCount = DefaultFrameCountToCapture, Exception ex = null,
            ICollection<string> filters = null, char indent = ' ')
        {
            List<Tuple<int, StackFrame>> orderedFrames = GetFilteredStacks(maxCount, ex, filters);
            if (orderedFrames == null || orderedFrames.Count == 0)
            {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < orderedFrames.Count; i++)
            {
                var orderedFrame = orderedFrames[i];
                int frameOrder = orderedFrame.Item1;
                string frameDesc = orderedFrame.Item2.ToString();
                sb.AppendFormat($"{new string(indent, i * 2)}[{frameOrder}]: {frameDesc}{Environment.NewLine}");
            }

            return sb.ToString();
        }
    }
}

