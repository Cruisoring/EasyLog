using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyLogger;

namespace EasyLoggerTest
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestLogLevel()
        {
            LogLevel lvl = Log.DefaultLogLevel;
            Console.WriteLine(lvl.ToString());

            lvl = lvl | LogLevel.VERBOSE;
            Console.WriteLine(lvl.ToString());

            lvl = LogLevel.VERBOSE | LogLevel.WARN_AND_ABOVE;
            Console.WriteLine(lvl.ToString());
            Assert.AreEqual("VERBOSE, WARN_AND_ABOVE", lvl.ToString());

            lvl = LogLevel.VERBOSE | LogLevel.WARN | LogLevel.INFO;
            Console.WriteLine(lvl.ToString());
            Assert.AreEqual("VERBOSE, INFO, WARN", lvl.ToString());

        }
    }
}
