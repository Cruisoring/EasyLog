using System;
using System.Collections.Generic;
using System.Text;

namespace Logger
{
    public enum LogLevel
    {
        // Priority constant for the println method; use Log.v.
        VERBOSE,

        // Priority constant for the println method; use Log.d.
        DEBUG,

        // Priority constant for the println method; use Log.i.
        INFO,

        // Priority constant for the println method; use Log.w.
        WARN,

        // Priority constant for the println method; use Log.e.
        ERROR,

        NONE,
    }
}
