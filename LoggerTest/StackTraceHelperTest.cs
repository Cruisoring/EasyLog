using System;
using System.Collections.Generic;
using System.Diagnostics;
using Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoggerTest
{
    [TestClass]
    public class StackTraceHelperTest
    {
        private List<Tuple<int, StackFrame>> StacksFromProperty => StackTraceHelper.GetFilteredStacks(10,
            filters: new HashSet<string>() { @"^System\.", @"^Microsoft\.", @"NUnit\." });

        private List<Tuple<int, StackFrame>> getFrames()
        {
            return StackTraceHelper.GetFilteredStacks(10, filters: new HashSet<string>() { @"^System\.", @"^Microsoft\.", @"NUnit\." });
        }

        [TestMethod]
        public void TestGetFilteredStacks()
        {
            List<Tuple<int, StackFrame>> frames = getFrames();
            frames.ForEach(f => Console.WriteLine(f.ToString()));
            Assert.AreEqual(3, frames.Count);
        }

        [TestMethod]
        public void TestGetFilteredStacksViaProperty()
        {
            List<Tuple<int, StackFrame>> frames = StacksFromProperty;
            frames.ForEach(f => Console.WriteLine(f.ToString()));
            Assert.AreEqual(3, frames.Count);
        }

        [TestMethod]
        public void TestGetFilteredStackString()
        {
            string stacks = StackTraceHelper.GetFilteredStackString(filters: new HashSet<string>() { @"^System\.", @"^Microsoft\.", @"NUnit\." });
            Console.WriteLine(stacks);
        }
    }
}
