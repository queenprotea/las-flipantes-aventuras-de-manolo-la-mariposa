using System;
using System.Globalization;

namespace Torres.ArchitectureSpike.Client
{
    internal sealed class CheckReport
    {
        private const string PassedLine = "[PASS] {0}: {1}";
        private const string FailedLine = "[FAIL] {0}: {1}";
        private const string SummaryLine = "{0} checks, {1} failed.";

        private int _checkCount;

        internal int FailureCount { get; private set; }

        internal void Record(string checkName, bool isPassed, string detail)
        {
            _checkCount++;
            string lineFormat = isPassed ? PassedLine : FailedLine;
            if (!isPassed)
            {
                FailureCount++;
            }

            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, lineFormat, checkName, detail));
        }

        internal void PrintSummary()
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, SummaryLine, _checkCount, FailureCount));
        }
    }
}
