namespace Scheduler.Util
{
    public interface IConsoleLogger
    {
        void LogInfo(string message);

        void LogMetrics(string message);

        void LogStatistics(string message);
    }
}
