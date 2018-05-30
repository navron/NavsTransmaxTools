using System;
using System.Diagnostics;
using Serilog;

namespace ProjectFixer.Utility
{
    /// <inheritdoc />
    /// <summary>
    /// A disposable timer that will log the time it takes for an action.
    /// </summary>
    public class LoggingTimer : IDisposable
    {
        public LoggingTimer(string action)
        {
            Action = action;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            Log.Information($"Running: {action}");
        }

        // Members
        private readonly Stopwatch stopwatch;

        /// <summary>
        /// The action being timed.
        /// </summary>
        public string Action { get; private set; }

        /// <summary>
        /// How long the action being timed took to execute.
        /// </summary>
        public long ElapsedMilliseconds => stopwatch.ElapsedMilliseconds;

        // Overtime handling
        private static TimeSpan? threshold;
        private static Action<LoggingTimer> overtimeCallback;

        public static void ConfigureOvertimeCallback(TimeSpan overtimeThreshold, Action<LoggingTimer> callback)
        {
            threshold = overtimeThreshold;
            overtimeCallback = callback;
        }

        public void Dispose()
        {
            stopwatch.Stop();
            Log.Information($"{Action} took {stopwatch.ElapsedMilliseconds} ms");

            // Check the overtime threshold
            if (threshold.HasValue && stopwatch.ElapsedMilliseconds > threshold.Value.TotalMilliseconds)
            {
                overtimeCallback(this);
            }
        }
    }
}
