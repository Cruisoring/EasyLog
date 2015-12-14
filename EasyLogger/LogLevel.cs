using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLogger
{
    [Flags]
    public enum LogLevel
    {
        NONE = 0,

        // Priority constant for the println method; use Log.v.
        VERBOSE = 1,

        // Priority constant for the println method; use Log.d.
        DEBUG = 2,

        // Priority constant for the println method; use Log.i.
        INFO = 4,

        // Priority constant for the println method; use Log.w.
        WARN = 8,

        // Priority constant for the println method; use Log.e.
        ERROR = 16, 

        WARN_AND_ABOVE = WARN | ERROR,
        INFO_AND_ABOVE = INFO | WARN_AND_ABOVE,
        DEBUG_AND_ABOVE = DEBUG | INFO_AND_ABOVE,
        ALL = VERBOSE | DEBUG_AND_ABOVE
    }
}
