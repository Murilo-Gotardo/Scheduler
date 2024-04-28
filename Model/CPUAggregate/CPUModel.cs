using Scheduler.Model.TaskSOAggregate;

namespace Scheduler.Model.CPUAggregate
{
    internal static class CPUModel
    {
        public static TaskSOModel? TaskSO { get; set; }

        public static double Utilization { get; set; }
    }
}
