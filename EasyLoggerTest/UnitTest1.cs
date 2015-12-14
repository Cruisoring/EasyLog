using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyLogger;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace EasyLoggerTest
{
    [TestClass]
    public class UnitTest1
    {
        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private void t1(int? v)
        {
            Log.D("t1");
            t2(v);
            Log.V("t1 done!");
        }

        private void t2(int? v)
        {
            if (v == null)
                throw new ArgumentNullException("Null Argument.");
            else if (v > 2)
            {
                Log.I("v=" + v);
                t3();
            }
            else
                Log.W("Too big v: " + v);
        }

        private void t3()
        {
            Log.V("verbose from t3");
            Log.E("To throw Exception in t3");
            Log.E(new Exception(), LogLevel.WARN);
        }

        [TestMethod]
        public void TestMethod1()
        {
            Log.V("verbose");
            Log.D("debug");
            Log.I("info");
            Log.W("warn");
            Log.E("error");
            try
            {
                t1(null);
            }
            catch(Exception ex)
            {
                Log.E(ex, LogLevel.INFO);
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            Log.I("Change the DefaultLogLevel to VERBOSE from" + Log.ChangeDefaultLogLevel(LogLevel.VERBOSE));
            Log debugLog = new Log(LogLevel.VERBOSE, 
                (level, msg) => TestContext.WriteLine(msg), 
                l => string.Format("@@@@<{0}-{1}>", l.ToString().Substring(0, 2), Log.ElapsedTimeString(@"mm\:ss\.ff")),
                Log.ByException,
                (tag, details) => tag + "__" + details);
            debugLog.i("Test debugLog.i");
            Log.V("verbose");
            Log.D("debug");
            Log.I("info");
            Log.W("warn");
            Log.E("error");
            try
            {
                throw new ArgumentException("some ex");
            }
            catch(Exception ex)
            {
                Log.E(ex, LogLevel.INFO);
            }

        }

        [TestMethod]
        public void TestMethod3()
        {
            Log.I("Change the DefaultLogLevel to " + Log.ChangeDefaultLogLevel(LogLevel.ALL));
            var memLog = new MemLog(LogLevel.VERBOSE | LogLevel.WARN,
                Log.FullTag,
                Log.DebuggableStacktrace
                );
            t1(3);

            Console.WriteLine("\r\n------------------------------------------");
            Console.WriteLine("MemLog shows:");
            Console.WriteLine(memLog.GetLog());
        }

        public IEnumerable<long> GetPrime1(int total, string momentId)
        {
            Log.RestartStopwatch();

            long[] primeNumbers = new long[total];

            long lastPrime = 1, nextPrime;
            long compareCount = 0;
            int last = Log.MarkMoment(momentId);
            for (int i = 0; i < total; i++)
            {
                switch (i)
                {
                    case 0:
                    case 1:
                        nextPrime = lastPrime + 1;
                        break;
                    default:
                        bool isPrime;
                        do
                        {
                            isPrime = true;
                            nextPrime = lastPrime + 2;
                            for (int j = 3; j < nextPrime / 2; j += 2, compareCount++)
                            {
                                if (nextPrime % j == 0)
                                {
                                    isPrime = false;
                                    break;
                                }
                            }
                            lastPrime = nextPrime;
                        } while (isPrime == false);
                        break;
                }
                primeNumbers[i] = nextPrime;
                lastPrime = nextPrime;
                //Log.I("{0}: {1} @ {2}", i, nextPrime, Log.MarkMoment(momentId));
                last = Log.MarkMoment(momentId);
            }

            Log.D(string.Format("End at {0}, with timestamp of {1} after comparing {2} times and mrked {3} times. \r\n"
                , DateTime.Now, Log.ElapsedTimeString(), compareCount, last+1));
            return primeNumbers;
        }

        public IEnumerable<long> GetPrime2(int total, string momentId)
        {
            Log.RestartStopwatch();
            long[] primeNumbers = new long[total];

            long lastPrime = 1, nextPrime;
            long compareCount = 0;
            int last = Log.MarkMoment(momentId);
            for (int i = 0; i < total; i++)
            {
                switch (i)
                {
                    case 0:
                    case 1:
                        nextPrime = lastPrime + 1;
                        break;
                    default:
                        bool isPrime;
                        do
                        {
                            isPrime = true;
                            nextPrime = lastPrime + 2;
                            long sqrtRoot = (long)Math.Sqrt(nextPrime);
                            for (long j = 0, k = primeNumbers[0]; k <= sqrtRoot; k = primeNumbers[++j], compareCount++)
                            {
                                if (nextPrime % k == 0)
                                {
                                    isPrime = false;
                                    break;
                                }
                            }
                            lastPrime = nextPrime;
                        } while (isPrime == false);
                        break;
                }
                primeNumbers[i] = nextPrime;
                lastPrime = nextPrime;
                //Log.I("{0}: {1} @ {2}", i, nextPrime, Log.MarkMoment("GetPrime"));
                last = Log.MarkMoment(momentId);
            }

            Log.D(string.Format("End at {0}, with timestamp of {1} after comparing {2} times and mrked {3} times. \r\n"
                , DateTime.Now, Log.ElapsedTimeString(), compareCount, last+1));
            return primeNumbers;
        }

        int startIndex = 80000, every = 5000, total = 100000;
        int[] indexes
        {
            get
            {
                List<int> list = new List<int>();
                int index = startIndex;
                do
                {
                    list.Add(index);
                    index += every;
                } while (index <= total);
                return list.ToArray();
            }
        }

        [TestMethod]
        public void TestPrimeNumber()
        {
            string label = "Test1";
            IEnumerable<long> primeNumbers1 = GetPrime1(total, label);
            Log.I(AverageInterval(label, total));
            long[] primes = indexes.Where(i => i <= total).Select(i => primeNumbers1.ElementAt(i - 1)).ToArray();
            //Log.D("The {0}th prime numer is {1}.\r\n", total, primeNumbers1.ElementAt(total-1));
            logElapsed(label, total, primes);

            label = "2";
            IEnumerable<long> primeNumbers2 = GetPrime2(total, label);
            Log.I(AverageInterval(label, total));
            long[] primes2 = indexes.Where(i => i <= total).Select(i => primeNumbers2.ElementAt(i - 1)).ToArray();
            Log.D("The {0}th prime numer is {1}\r\n.", total, primeNumbers2.ElementAt(total - 1));
            logElapsed(label, total, primes2);

            Assert.IsTrue(primeNumbers1.SequenceEqual(primeNumbers2));
        }

        private void logElapsed(string label, int total, long[] primes)
        {

            long[] moments = Log.GetMoments(label, indexes);
            long[] previousMoments = Log.GetMoments(label, indexes.Select(i => i - 1));
            long[] expectedIntervals = moments.Select((m, index) => m - previousMoments[index]).ToArray();

            long[] intervals = Log.GetIntervals(label, indexes);
            Log.D("#No\tPrimeNumber\t\tInterval\t\tExpected");
            for(int i = 0; i<moments.Count(); i++)
            {
                Log.D("{0}\t{1}\t\t{2}\t\t{3}", 
                    indexes[i], primes[i], intervals[i], expectedIntervals[i]);
            }
            Assert.IsTrue(expectedIntervals.SequenceEqual(intervals));
        }

        string AverageInterval(string momentId, int total)
        {
            long[] startEnd = Log.GetMoments(momentId, new int[] { 0, total });

            long ticks = startEnd[1] - startEnd[0];

            long averageTicks = ticks / total;
            return string.Format("Total: {0} of {1} intervals, average is {2}ns.", new TimeSpan(ticks),
                total, (int)(averageTicks * 10));
        }
    }
}
