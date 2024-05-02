using Scheduler.Model.TaskSOAggregate;

namespace Scheduler.Model.CPUAggregate
{
    internal static class CpuModel
    {
        public static TaskSoModel? TaskSo { get; set; }

        public static double Utilization { get; set; }
    }
}
